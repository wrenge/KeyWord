using System.Threading.Tasks;

namespace KeyWord.Client.Application.Services
{
    public interface IMediaPlayer
    {
        Task SetDataSource(string filepath);
        void Reset();
        Task Prepare();
        void Start();
    }
}