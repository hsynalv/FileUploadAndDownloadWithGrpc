using Grpc.Net.Client;
using grpcFileTransportDownloadClient;

var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new FileService.FileServiceClient(channel);

string downloadPath = @"C:\Users\beyazskorsky\Desktop\download\";

var fileInfo = new grpcFileTransportDownloadClient.FileInfo()
{
    FileName = "gRPC",
    FileExtension = ".pdf"
};

var request = client.FileDownload(fileInfo);
FileStream fileStream = null;

int count = 0;
decimal chunkSize = 0;

while (await request.ResponseStream.MoveNext(new CancellationTokenSource().Token))
{
    if (count++ == 0)
    {
        fileStream = new FileStream($"{downloadPath}/{request.ResponseStream.Current.Info.FileName}{request.ResponseStream.Current.Info.FileExtension}",FileMode.CreateNew);
        
        fileStream.SetLength(request.ResponseStream.Current.FileSize);
    }

    var buffer = request.ResponseStream.Current.Buffer.ToByteArray();
    await fileStream.WriteAsync(buffer, 0, request.ResponseStream.Current.ReadedByte);

    Console.WriteLine($"{Math.Round(((chunkSize += request.ResponseStream.Current.ReadedByte) * 100) / request.ResponseStream.Current.FileSize)}%");
}

Console.WriteLine("Download Başarılı");
await fileStream.DisposeAsync();
fileStream.Close();