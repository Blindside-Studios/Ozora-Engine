using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ozora
{
    public class Initializer
    {
        public Vector3[] GenerateCloudPositions(CloudSettings preferences) 
        {
            double workableWidth = preferences.AreaWidth - preferences.ImageWidth;
            double workableHeight = preferences.AreaHeight - preferences.ImageHeight;

            int spawnableCloudsAmount = Convert.ToInt32(Math.Round(
                (workableWidth * workableHeight) / (preferences.ImageWidth * preferences.ImageHeight) * preferences.DensityModifier));

            Random random = new Random();
            Vector3[] _vectorsList = new Vector3[spawnableCloudsAmount];

            for (int i = 0; i < spawnableCloudsAmount; i++)
            {
                float _spawnWidth = (float)(random.NextDouble() * workableWidth);
                float _spawnHeight = (float)(random.NextDouble() * workableHeight);
                Vector3 _spawnVector = new Vector3(_spawnWidth, _spawnHeight, 0);
                _vectorsList[i] = _spawnVector;
            }

            return _vectorsList;
        }
    }

    public class CloudSettings
    {
        public double AreaWidth { get; set; }
        public double AreaHeight { get; set; }
        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        public double DensityModifier { get; set; }
    }
}
