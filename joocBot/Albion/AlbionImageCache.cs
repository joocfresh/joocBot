using System.Collections.Concurrent;
using System.Drawing;
using System.Net;
using System.Security.Cryptography;

namespace joocBot.Albion
{
    static class AlbionImageCache
    {
        private static readonly HttpClient _http = new HttpClient(
            new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private static readonly string _cacheDir = Path.Combine(AppContext.BaseDirectory, "cache", "albion_render");
        private static readonly TimeSpan _ttl = TimeSpan.FromDays(7);

        public static async Task<Image> LoadImageCachedAsync(string url, int placeholderCellSize)
        {
            if (string.IsNullOrWhiteSpace(url))
                return CreateNoImagePlaceholder(placeholderCellSize, "NO IMAGE");

            Directory.CreateDirectory(_cacheDir);

            string filePath = Path.Combine(_cacheDir, Sha1(url) + ".png");

            // 1) 캐시 히트
            if (File.Exists(filePath))
            {
                var age = DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath);
                if (age <= _ttl)
                    return LoadBitmapFromFileSafe(filePath, placeholderCellSize);
            }

            // 2) 동시 다운로드 방지 (URL 단위 Lock)
            var sem = _locks.GetOrAdd(url, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            try
            {
                // 다른 스레드가 먼저 받아뒀을 수 있으니 재확인
                if (File.Exists(filePath))
                {
                    var age = DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath);
                    if (age <= _ttl)
                        return LoadBitmapFromFileSafe(filePath, placeholderCellSize);
                }

                // 3) 다운로드
                using var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!resp.IsSuccessStatusCode)
                    return CreateNoImagePlaceholder(placeholderCellSize, "NO IMAGE");

                await using var net = await resp.Content.ReadAsStreamAsync();
                await using var fs = File.Create(filePath);
                await net.CopyToAsync(fs);

                // 4) 파일에서 로드(파일잠금 방지 방식)
                return LoadBitmapFromFileSafe(filePath, placeholderCellSize);
            }
            catch
            {
                return CreateNoImagePlaceholder(placeholderCellSize, "NO IMAGE");
            }
            finally
            {
                sem.Release();
            }
        }

        private static string Sha1(string s)
        {
            using var sha1 = SHA1.Create();
            var bytes = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));
            return Convert.ToHexString(bytes);
        }

        private static Image LoadBitmapFromFileSafe(string path, int placeholderCellSize)
        {
            try
            {
                // Image.FromFile은 파일 락을 오래 잡는 경우가 있어 바이트로 읽어서 Bitmap 고정 권장
                var bytes = File.ReadAllBytes(path);
                using var ms = new MemoryStream(bytes);
                using var img = Image.FromStream(ms);
                return new Bitmap(img);
            }
            catch
            {
                return CreateNoImagePlaceholder(placeholderCellSize, "NO IMAGE");
            }
        }

        // 연구원님 기존 CreateNoImagePlaceholder 그대로 사용 가능
        private static Bitmap CreateNoImagePlaceholder(int size, string text)
            => new Bitmap(size, size);
    }
}
