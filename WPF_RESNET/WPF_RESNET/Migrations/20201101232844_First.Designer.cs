﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WPF_RESNET;

namespace WPF_RESNET.Migrations
{
    [DbContext(typeof(LibraryContext))]
    [Migration("20201101232844_First")]
    partial class First
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0-rc.2.20475.6");

            modelBuilder.Entity("WPF_RESNET.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("FileDetailsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Hash")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NumberOfRequests")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TypeId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("FileDetailsId");

                    b.HasIndex("TypeId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("WPF_RESNET.FileDetails", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Data")
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("FileDetails");
                });

            modelBuilder.Entity("WPF_RESNET.Type", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TypeName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Types");
                });

            modelBuilder.Entity("WPF_RESNET.File", b =>
                {
                    b.HasOne("WPF_RESNET.FileDetails", "FileDetails")
                        .WithMany()
                        .HasForeignKey("FileDetailsId");

                    b.HasOne("WPF_RESNET.Type", "Type")
                        .WithMany("Files")
                        .HasForeignKey("TypeId");

                    b.Navigation("FileDetails");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("WPF_RESNET.Type", b =>
                {
                    b.Navigation("Files");
                });
#pragma warning restore 612, 618
        }
    }
}
