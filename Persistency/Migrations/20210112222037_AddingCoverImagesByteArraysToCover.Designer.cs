﻿// <auto-generated />
using System;
using Covers.Persistency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Covers.Persistency.Migrations
{
    [DbContext(typeof(CoversContext))]
    [Migration("20210112222037_AddingCoverImagesByteArraysToCover")]
    partial class AddingCoverImagesByteArraysToCover
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Covers.Persistency.Entities.Album", b =>
                {
                    b.Property<long>("AlbumId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.HasKey("AlbumId");

                    b.ToTable("Albums");
                });

            modelBuilder.Entity("Covers.Persistency.Entities.Artist", b =>
                {
                    b.Property<long>("ArtistId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ArtistId");

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("Covers.Persistency.Entities.Cover", b =>
                {
                    b.Property<long>("CoverId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AlbumId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("BackCover")
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("FrontCover")
                        .HasColumnType("BLOB");

                    b.HasKey("CoverId");

                    b.HasIndex("AlbumId")
                        .IsUnique();

                    b.ToTable("Covers");
                });

            modelBuilder.Entity("Covers.Persistency.Entities.Track", b =>
                {
                    b.Property<long>("TrackId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("AlbumId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("ArtistId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .HasColumnType("TEXT");

                    b.HasKey("TrackId");

                    b.HasIndex("AlbumId");

                    b.HasIndex("ArtistId");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("Covers.Persistency.Entities.Cover", b =>
                {
                    b.HasOne("Covers.Persistency.Entities.Album", "Album")
                        .WithOne("Cover")
                        .HasForeignKey("Covers.Persistency.Entities.Cover", "AlbumId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Album");
                });

            modelBuilder.Entity("Covers.Persistency.Entities.Track", b =>
                {
                    b.HasOne("Covers.Persistency.Entities.Album", "Album")
                        .WithMany("Tracks")
                        .HasForeignKey("AlbumId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Covers.Persistency.Entities.Artist", "Artist")
                        .WithMany("Tracks")
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Album");

                    b.Navigation("Artist");
                });

            modelBuilder.Entity("Covers.Persistency.Entities.Album", b =>
                {
                    b.Navigation("Cover");

                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("Covers.Persistency.Entities.Artist", b =>
                {
                    b.Navigation("Tracks");
                });
#pragma warning restore 612, 618
        }
    }
}
