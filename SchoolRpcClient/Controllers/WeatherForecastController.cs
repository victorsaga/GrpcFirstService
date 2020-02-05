using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchoolRpc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolRpcClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GRPCController : ControllerBase
    {
        private readonly ILogger<GRPCController> _logger;

        public GRPCController(ILogger<GRPCController> logger)
        {
            _logger = logger;
        }

        [HttpPost("GetStudent")]
        public Student GetStudent([FromBody]StudentRequest request)
        {
            var client = new StudentProfile.StudentProfileClient(GrpcChannel.ForAddress("https://localhost:5001"));
            return client.GetStudent(new StudentRequest { });
        }

        [HttpPost("GetMultiStudent")]
        public async Task<IEnumerable<Student>> GetMultiStudentAsync([FromBody]IList<int> ids)
        {
            var client = new StudentProfile.StudentProfileClient(GrpcChannel.ForAddress("https://localhost:5001"));
            var responses = new List<Student>();
            using (var stream = client.GetMultiStudent())
            {
                //await stream.WriteAllAsync(new StudentRequest() { Id = 1 });

                foreach (var item in ids)
                {
                    await stream.RequestStream.WriteAsync(new StudentRequest() { Id = item });
                }
                await stream.RequestStream.CompleteAsync();
                while (await stream.ResponseStream.MoveNext(new CancellationToken()))
                {
                    responses.Add(stream.ResponseStream.Current);
                }
            }
            return responses;
        }

    }
}
