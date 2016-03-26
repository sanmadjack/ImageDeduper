using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator.Identifiers {
    class FileHashIdentifier: AHashIdentifier {
        public FileHashIdentifier(byte[] image_bytes) : base(image_bytes) { }
    }
}
