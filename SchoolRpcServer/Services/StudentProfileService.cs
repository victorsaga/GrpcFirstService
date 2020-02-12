using Grpc.Core;
using Grpc.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolRpc
{
    public class StudentProfileService : StudentProfile.StudentProfileBase
    {
        public StudentProfileService()
        {
        }

        public override Task<Student> GetStudent(StudentRequest request, ServerCallContext context)
        {
            return Task.FromResult<Student>(new Student()
            {
                Id = request.Id,
                Gender = request.Gender,
                Name = request.Name,
                ResponseTime = GetDateTimeNowString()
            });
        }

        public override Task<BaseResponse> GetStudentReturnBaseResponse(StudentRequest request, ServerCallContext context)
        {
            return Task.FromResult<BaseResponse>(

                new BaseResponse()
                {
                    Success = true,
                    Code = "00000",
                    DataString = JsonConvert.SerializeObject(new Student()
                    {
                        Id = request.Id,
                        Gender = request.Gender,
                        Name = request.Name,
                        ResponseTime = GetDateTimeNowString()
                    })
                });
        }

        public override async Task GetStudentList(IAsyncStreamReader<StudentRequest> requestStream, IServerStreamWriter<Student> responseStream, ServerCallContext context)
        {
            await foreach (var item in requestStream.ReadAllAsync())
                await responseStream.WriteAsync(new Student() { Id = item.Id, ResponseTime = GetDateTimeNowString() });
        }

        /// <summary>
        /// 叫用Utils中的method來跑loop
        /// https://github.com/grpc/grpc/blob/master/src/csharp/Grpc.Core/Utils/AsyncStreamExtensions.cs
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task GetStudentListExtention(IAsyncStreamReader<StudentRequest> requestStream, IServerStreamWriter<Student> responseStream, ServerCallContext context)
        {
            var request = await requestStream.ToListAsync();
            var students = request.Select(x => new Student
            {
                Gender = x.Gender,
                Id = x.Id,
                Name = x.Name,
                ResponseTime = GetDateTimeNowString()
            });
            await responseStream.WriteAllAsync<Student>(students);
        }

        private string GetDateTimeNowString() => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
    }
}
