using Grpc.Core.Utils;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchoolRpc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolRpcClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GrpcController : ControllerBase
    {
        private readonly ILogger<GrpcController> _logger;

        public GrpcController(ILogger<GrpcController> logger)
        {
            _logger = logger;
        }

        [HttpPost("GetStudent")]
        public Student GetStudent([FromBody]StudentRequest request) //{ "id":1922, "name":"vic" }
        {
            var client = new StudentProfile.StudentProfileClient(GrpcChannel.ForAddress("https://localhost:5001"));
            return client.GetStudent(request);
        }

        [HttpPost("GetStudentBaseResponse")]
        public BaseResponse GetStudentBaseResponse([FromBody]StudentRequest request) //{ "id":1922, "name":"vic" }
        {
            var client = new StudentProfile.StudentProfileClient(GrpcChannel.ForAddress("https://localhost:5001"));
            return client.GetStudentReturnBaseResponse(request);
        }


        [HttpPost("GetStudentList")]
        public async Task<IEnumerable<Student>> GetMultiStudentListAsync([FromBody]IList<int> ids) //[9,10]
        {
            var client = new StudentProfile.StudentProfileClient(GrpcChannel.ForAddress("https://localhost:5001"));
            var responses = new List<Student>();
            using (var stream = client.GetStudentList())
            {
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

        [HttpPost("GetStudentListExtenstion")]
        public async Task<IEnumerable<Student>> GetStudentListExtenstion([FromBody]IList<int> ids)  //[9,10]
        {
            var client = new StudentProfile.StudentProfileClient(GrpcChannel.ForAddress("https://localhost:5001"));
            var responses = new List<Student>();
            using (var stream = client.GetStudentList())
            {
                await stream.RequestStream.WriteAllAsync(ids.Select(x => new StudentRequest() { Id = x }));
                responses = await stream.ResponseStream.ToListAsync();
            }
            return responses;
        }

    }
}
