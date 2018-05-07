﻿// <auto-generated />
using System;
using LinkEat.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LinkEat.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20180501131636_RelationFix")]
    partial class RelationFix
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-preview2-30571")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LinkEat.Models.Appointment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<int>("PlaceId");

                    b.Property<bool>("Reminded");

                    b.Property<bool>("RemindedDepart");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Appointments");
                });

            modelBuilder.Entity("LinkEat.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("LinkEat.Models.Meal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Dish");

                    b.Property<int>("PlaceId");

                    b.Property<double>("Price");

                    b.Property<int?>("SauceId");

                    b.HasKey("Id");

                    b.ToTable("Meals");
                });

            modelBuilder.Entity("LinkEat.Models.MealVegetable", b =>
                {
                    b.Property<int>("MealId");

                    b.Property<int>("VegetableId");

                    b.HasKey("MealId", "VegetableId");

                    b.HasIndex("VegetableId");

                    b.ToTable("MealVegetable");
                });

            modelBuilder.Entity("LinkEat.Models.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Confirmed");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Meal");

                    b.Property<int>("MealId");

                    b.Property<int>("PlaceId");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("LinkEat.Models.Place", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CategoryId");

                    b.Property<string>("Name");

                    b.Property<string>("OrderingAPI");

                    b.Property<string>("Phone");

                    b.Property<string>("Website");

                    b.HasKey("Id");

                    b.ToTable("Places");
                });

            modelBuilder.Entity("LinkEat.Models.Sauce", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Sauces");
                });

            modelBuilder.Entity("LinkEat.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<string>("SlackId");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("LinkEat.Models.Vegetable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Vegetables");
                });

            modelBuilder.Entity("LinkEat.Models.MealVegetable", b =>
                {
                    b.HasOne("LinkEat.Models.Meal", "Meal")
                        .WithMany("MealVegetables")
                        .HasForeignKey("MealId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("LinkEat.Models.Vegetable", "Vegetable")
                        .WithMany("MealVegetables")
                        .HasForeignKey("VegetableId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}