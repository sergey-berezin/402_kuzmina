﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models;

namespace UIRecognition.Migrations
{
    [DbContext(typeof(DbRepository))]
    [Migration("20211122090422_minusConcurrency")]
    partial class minusConcurrency
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.12");

            modelBuilder.Entity("Models.RecognizedImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Hash")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Image")
                        .IsConcurrencyToken()
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("RecognizedImages");
                });

            modelBuilder.Entity("Models.RecognizedObject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Class")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<int?>("RecognizedImageId")
                        .HasColumnType("INTEGER");

                    b.Property<float>("X1")
                        .IsConcurrencyToken()
                        .HasColumnType("REAL");

                    b.Property<float>("X2")
                        .IsConcurrencyToken()
                        .HasColumnType("REAL");

                    b.Property<float>("Y1")
                        .IsConcurrencyToken()
                        .HasColumnType("REAL");

                    b.Property<float>("Y2")
                        .IsConcurrencyToken()
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("RecognizedImageId");

                    b.ToTable("RecognizedObjects");
                });

            modelBuilder.Entity("Models.RecognizedObject", b =>
                {
                    b.HasOne("Models.RecognizedImage", null)
                        .WithMany("Objects")
                        .HasForeignKey("RecognizedImageId");
                });

            modelBuilder.Entity("Models.RecognizedImage", b =>
                {
                    b.Navigation("Objects");
                });
#pragma warning restore 612, 618
        }
    }
}