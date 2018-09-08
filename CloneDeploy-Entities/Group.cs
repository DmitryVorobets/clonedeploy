﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration;

namespace CloneDeploy_Entities
{
    [Table("groups", Schema = "public")]
    public class GroupEntity
    {
        public GroupEntity()
        {
            ImageId = -1;
            ImageProfileId = -1;
            SetDefaultProperties = 0;
            SetDefaultBootMenu = 0;
        }

        [Column("cluster_group_id", Order = 10)]
        public int ClusterGroupId { get; set; }

        [Column("group_description", Order = 3)]
        public string Description { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("group_id", Order = 1)]
        public int Id { get; set; }

        [Column("group_image_id", Order = 4)]
        public int ImageId { get; set; }

        [Column("group_image_profile_id", Order = 5)]
        public int ImageProfileId { get; set; }

        [Column("group_name", Order = 2)]
        public string Name { get; set; }

        [Column("group_default_bootmenu_enabled", Order = 9)]
        public int SetDefaultBootMenu { get; set; }

        [Column("group_default_properties_enabled", Order = 8)]
        public int SetDefaultProperties { get; set; }

        [Column("group_smart_criteria", Order = 7)]
        public string SmartCriteria { get; set; }

        [Column("group_type", Order = 6)]
        public string Type { get; set; }

        [Column("smart_type", Order = 11)]
        public string SmartType { get; set; }
    }

    [NotMapped]
    public class GroupWithImage : GroupEntity
    {
        public ImageEntity Image { get; set; }
    }

    public sealed class GroupCsvMap : ClassMap<GroupEntity>
    {
        public GroupCsvMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Description).Name("Description");
            Map(m => m.Type).Name("Type");
            Map(m => m.SmartCriteria).Name("SmartCriteria");
        }
    }
}