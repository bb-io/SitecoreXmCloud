using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreXmCloud.Models.Requests.Item
{
    public class FilteringOptions
    {
        [Display("Exclude fields where Value property equals")]
        public IEnumerable<string>? Value { get; set; }


        [Display("Exclude fields where Type property equals")]
        public IEnumerable<string>? Type { get; set; }


        [Display("Exclude fields where TypeKey property equals")]
        public IEnumerable<string>? TypeKey { get; set; }


        //[Display("Exclude fields where Definition property equals")]
        //public IEnumerable<string>? Definition { get; set; }


        //[Display("Exclude fields where Description property equals")]
        //public IEnumerable<string>? Description { get; set; }


        [Display("Exclude fields where DisplayName property equals")]
        public IEnumerable<string>? DisplayName { get; set; }


        [Display("Exclude fields where Name property equals")]
        public IEnumerable<string>? Name { get; set; }


        [Display("Exclude fields where Key property equals")]
        public IEnumerable<string>? Key { get; set; }


        [Display("Exclude fields where Section property equals")]
        public IEnumerable<string>? Section { get; set; }


        //[Display("Exclude fields where SectionDisplayName property equals")]
        //public IEnumerable<string>? SectionDisplayName { get; set; }


        [Display("Exclude fields where Title property equals")]
        public IEnumerable<string>? Title { get; set; }
    }
}
