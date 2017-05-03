using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Localhost.Apis.Gallery.v0_1;
using Localhost.Apis.Gallery.v0_1.Data;

namespace ImageDeduplicator.ImageSources {
    public class DartleryImageSource : AImageSource {


        String tags = String.Empty;
        DateTime? cutoffDate;
        String address;
        String imagePath;
        GalleryService service;

        public DartleryImageSource(String address, String user, String password, String tags, DateTime? cutoffDate, String imagePath) : 
            base(tags) {
            this.address = address;
            this.tags = tags;
            this.imagePath = imagePath;
            this.cutoffDate = cutoffDate;

            service = new GalleryService();
        }

        
        private long ToUnixTime(DateTime date)
        {
            dynamic epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        private PaginatedResponse getPageOfItems(int page)
        {
            if(String.IsNullOrEmpty(tags))
            {
                ItemsResource.GetVisibleIdsRequest request = service.Items.GetVisibleIds();
                request.Page = page;
                if(cutoffDate.HasValue)
                    request.CutoffDate = cutoffDate.ToString();
                return request.Execute();
            } else
            {
                ItemSearchRequest searchRequest = new ItemSearchRequest();
                searchRequest.Tags = getTags();
                searchRequest.Page = page;
                if (cutoffDate.HasValue)
                   searchRequest.CutoffDate = cutoffDate.ToString();

                ItemsResource.SearchVisibleRequest request = service.Items.SearchVisible(searchRequest);
                return request.Execute();
            }
        }

        public override List<ComparableImage> getImages()
        {
            List<ComparableImage> output = new List<ComparableImage>();


            PaginatedResponse response = getPageOfItems(0);

            while(true)
            {
                foreach (String id in response.Items)
                {
                    String filePath = Path.Combine(imagePath, "originals", id.Substring(0,2), id);
                    String thumbnailPath = Path.Combine(imagePath, "thumbnails", id.Substring(0, 2), id);
                    ComparableImage img = new ComparableImage(this, filePath, thumbnailPath);
                    img.InternalIdentifier = id;
                    output.Add(img);
                }
                if(response.Page+1>=response.PageCount)
                {
                    break;
                } else {
                    response = getPageOfItems(response.Page.Value + 1);
                }
            }

            return output;
        }

        public override void deleteImage(ComparableImage image)
        {
            ItemsResource.DeleteRequest request = service.Items.Delete(image.InternalIdentifier.ToString());
            request.Execute();
        }

        public override ComparableImage mergeImages(ComparableImage source, ComparableImage target)
        {
            IdRequest sourceId = new IdRequest();
            sourceId.Id = source.InternalIdentifier.ToString();

            ItemsResource.MergeItemsRequest request = service.Items.MergeItems(sourceId, target.InternalIdentifier.ToString());

            return target;
        }

        protected override void deleteImageFromDisk(ComparableImage image)
        {
            
        }
        protected override List<string> getImagesInternal()
        {
            throw new NotImplementedException();
        }

        private List<Tag> getTags() {
            List<Tag> output = new List<Tag>();
            foreach (String tag in tags.Split(' ')) {
                if (String.IsNullOrWhiteSpace(tag))
                    continue;
                Tag t = new Tag();
                t.Id = tag;
                output.Add(t);
            }
            return output;
        }


    }
}
