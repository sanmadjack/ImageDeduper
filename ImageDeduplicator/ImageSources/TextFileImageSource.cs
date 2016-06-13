using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;


namespace ImageDeduplicator.ImageSources {
    public class TextFileImageSource: AImageSource {

        private string file;

        public TextFileImageSource(String file): base(file) {
            this.file = file;
        }

        protected override List<String> getImagesInternal() {
            List<String> output = new List<String>();
            FileInfo fi = new FileInfo(file);
            if (fi.Extension.ToLower() == ".txt") {
                List<String> filesToLoad = new List<string>();
                using (StreamReader reader = fi.OpenText()) {
                    output.Add(reader.ReadLine());
                }
            } else if (fi.Extension.ToLower() == ".csv") {
                List<String[]> data = parseCSV(fi.FullName);
                List<String> filesToLoad = new List<string>();
                foreach (String[] line in data) {
                    foreach (String field in line) {
                        output.Add(field);
                    }
                }
            } else {
                throw new Exception("Text file format not supported");
            }
            return output;
        }

        public List<String[]> parseCSV(string path) {
            List<string[]> parsedData = new List<string[]>();
            string[] fields;

            TextFieldParser parser = new TextFieldParser(path);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            while (!parser.EndOfData) {
                fields = parser.ReadFields();
                parsedData.Add(fields);
            }

            parser.Close();


            return parsedData;
        }
    }
}
