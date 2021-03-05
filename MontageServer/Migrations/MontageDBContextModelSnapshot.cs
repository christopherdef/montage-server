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
                .HasAnnotation("ProductVersion", "3.1.11")
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

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ProjectId", "ClipId");

                    b.HasIndex("ClipId");

                    b.HasIndex("ProjectId", "Id");

                    b.ToTable("ClipAssignment");
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
                        .HasForeignKey("ProjectId", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
