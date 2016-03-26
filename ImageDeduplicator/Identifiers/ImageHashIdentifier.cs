using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageDeduplicator.Identifiers {
    class ImageHashIdentifier:AHashIdentifier {
        public ImageHashIdentifier(Image image) : 
            base(new ImageConverter().ConvertTo(image, typeof(byte[])) as byte[]) { }
    }
}
