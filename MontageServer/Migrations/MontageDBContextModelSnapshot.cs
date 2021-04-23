﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MontageServer.Data;

namespace MontageServer.Migrations
{
    [DbContext(typeof(MontageDbContext))]
    partial class MontageDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MontageServer.Models.AdobeClip", b =>
                {
                    b.Property<string>("ClipId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AnalysisResultString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FootagePath")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ClipId");

                    b.ToTable("AdobeClip");
                });

            modelBuilder.Entity("MontageServer.Models.AdobeProject", b =>
                {
                    b.Property<string>("ProjectId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ProjectId", "UserId");

                    b.ToTable("AdobeProject");
                });

            modelBuilder.Entity("MontageServer.Models.AnalysisResult", b =>
                {
                    b.Property<string>("ClipId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Error")
                        .HasColumnType("bit");

                    b.Property<string>("FootagePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Transcript")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ClipId");

                    b.ToTable("AnalysisResult");
                });

            modelBuilder.Entity("MontageServer.Models.ClipAssignment", b =>
                {
                    b.Property<string>("ProjectId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ClipId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ProjectId", "ClipId", "UserId");

                    b.HasIndex("ClipId");

                    b.HasIndex("ProjectId", "UserId");

                    b.ToTable("ClipAssignment");
                });

            modelBuilder.Entity("MontageServer.Models.SpeakerProfile", b =>
                {
                    b.Property<string>("SpeakerId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProjectId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ModelPath")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SpeakerId", "ProjectId", "UserId");

                    b.HasIndex("ProjectId", "UserId");

                    b.ToTable("SpeakerProfiles");
                });

            modelBuilder.Entity("MontageServer.Models.ClipAssignment", b =>
                {
                    b.HasOne("MontageServer.Models.AdobeClip", "Clip")
                        .WithMany()
                        .HasForeignKey("ClipId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MontageServer.Models.AdobeProject", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MontageServer.Models.SpeakerProfile", b =>
                {
                    b.HasOne("MontageServer.Models.AdobeProject", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
