using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Media;
using KeyWord.Client.Application.Droid;
using KeyWord.Client.Application.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidMediaPlayer))]

namespace KeyWord.Client.Application.Droid
{
    public class AndroidMediaPlayer : IMediaPlayer, IDisposable
    {
        private readonly MediaPlayer _player;
        private readonly SemaphoreSlim _prepareSemaphore;
        private AssetFileDescriptor _fileDescriptor;

        public AndroidMediaPlayer()
        {
            _player = new MediaPlayer();
            _player.Reset();
            _player.Prepared += (sender, args) => { _prepareSemaphore?.Release(); };
            _prepareSemaphore = new SemaphoreSlim(0, 1);
        }

        public async Task SetDataSource(string filepath)
        {
            _fileDescriptor = Android.App.Application.Context.Assets!.OpenFd(filepath);
            await _player.SetDataSourceAsync(_fileDescriptor.FileDescriptor, _fileDescriptor.StartOffset, _fileDescriptor.Length);
        }

        public void Reset()
        {
            _player.Reset();
        }

        public async Task Prepare()
        {
            _player.PrepareAsync();
            await _prepareSemaphore.WaitAsync();
        }

        public void Start()
        {
            _player.Start();
        }

        public void Dispose()
        {
            _player?.Dispose();
            _fileDescriptor?.Dispose();
        }
    }
}