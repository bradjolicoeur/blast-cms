using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TinifyAPI;

namespace blastcms.ImageResizeService
{
    public class TinifyService : ITinifyService
    {

        public TinifyService(IConfiguration configuration)
        {
            Tinify.Key = configuration["TINIFY_API_KEY"];
        }

        public async Task<byte[]> OptomizeFile(byte[] buffer)
        {
            var source = await Tinify.FromBuffer(buffer);

            var resized = source.Resize(new
            {
                method = "scale",
                width = 800
            });

            return await resized.ToBuffer();
        }
        public async Task<byte[]> OptomizeFile(string url)
        {
            var source = await Tinify.FromUrl(url);

            var resized = source.Resize(new
            {
                method = "scale",
                width = 800
            });

            return await resized.ToBuffer();
        }

    }
}
