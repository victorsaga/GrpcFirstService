using Grpc.Core;
using Grpc.Core.Utils;
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

        public override async Task GetMultiStudent(IAsyncStreamReader<StudentRequest> requestStream, IServerStreamWriter<Student> responseStream, ServerCallContext context)
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
        public override async Task GetMultiStudentCoreUtils(IAsyncStreamReader<StudentRequest> requestStream, IServerStreamWriter<Student> responseStream, ServerCallContext context)
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

        public override Task<Student> GetStudent(StudentRequest request, ServerCallContext context)
        {
            return Task.FromResult<Student>(new Student()
            {
                ResponseTime = GetDateTimeNowString()
            });
        }
        private string GetDateTimeNowString() => DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
    }
}
