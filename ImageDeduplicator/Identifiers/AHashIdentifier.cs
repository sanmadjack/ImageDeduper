using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ImageDeduplicator.Identifiers {
    class AHashIdentifier : AIdentifier {
        public readonly byte[] Hash;

        public AHashIdentifier(byte[] image_bytes) {
            using (var md5 = MD5.Create()) {
                Hash = md5.ComputeHash(image_bytes);
            }
        }

        public bool IsMatch(AHashIdentifier other) {
            return StructuralComparisons.StructuralEqualityComparer.Equals(Hash, other.Hash);
        }
    }
}
