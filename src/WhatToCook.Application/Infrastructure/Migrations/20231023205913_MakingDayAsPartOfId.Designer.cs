﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WhatToCook.Application.Infrastructure;

#nullable disable

namespace WhatToCook.Application.Infrastructure.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20231023205913_MakingDayAsPartOfId")]
    partial class MakingDayAsPartOfId
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WhatToCook.Application.Domain.Favourite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("RecipeId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.HasIndex("UserId");

                    b.ToTable("Favourite");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Ingredient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("RecipeId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.ToTable("Ingredient");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.PlanOfMeals", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("FromDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ToDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("PlanOfMeals");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Rating", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("RecipeId")
                        .HasColumnType("integer");

                    b.Property<int>("Score")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.HasIndex("UserId");

                    b.ToTable("Rating");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Recipe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TimeToPrepare")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.RecipePlanOfMeals", b =>
                {
                    b.Property<int>("RecipeId")
                        .HasColumnType("integer");

                    b.Property<int>("PlanOfMealsId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Day")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("RecipeId", "PlanOfMealsId", "Day");

                    b.HasIndex("PlanOfMealsId");

                    b.ToTable("RecipePlanOfMeals");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Tag");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Favourite", b =>
                {
                    b.HasOne("WhatToCook.Application.Domain.Recipe", "Recipe")
                        .WithMany()
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WhatToCook.Application.Domain.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Recipe");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Ingredient", b =>
                {
                    b.HasOne("WhatToCook.Application.Domain.Recipe", "Recipe")
                        .WithMany("Ingredients")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.PlanOfMeals", b =>
                {
                    b.HasOne("WhatToCook.Application.Domain.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Rating", b =>
                {
                    b.HasOne("WhatToCook.Application.Domain.Recipe", "Recipe")
                        .WithMany()
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WhatToCook.Application.Domain.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Recipe");

                    b.Navigation("User");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Recipe", b =>
                {
                    b.OwnsOne("WhatToCook.Application.Domain.Statistics", "Statistics", b1 =>
                        {
                            b1.Property<int>("RecipeId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .HasColumnType("integer");

                            b1.Property<int>("Shares")
                                .HasColumnType("integer");

                            b1.Property<int>("Views")
                                .HasColumnType("integer");

                            b1.HasKey("RecipeId");

                            b1.ToTable("Recipes");

                            b1.WithOwner()
                                .HasForeignKey("RecipeId");
                        });

                    b.Navigation("Statistics")
                        .IsRequired();
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.RecipePlanOfMeals", b =>
                {
                    b.HasOne("WhatToCook.Application.Domain.PlanOfMeals", "PlanOfMeals")
                        .WithMany("RecipePlanOfMeals")
                        .HasForeignKey("PlanOfMealsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WhatToCook.Application.Domain.Recipe", "Recipe")
                        .WithMany("RecipePlanOfMeals")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlanOfMeals");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.PlanOfMeals", b =>
                {
                    b.Navigation("RecipePlanOfMeals");
                });

            modelBuilder.Entity("WhatToCook.Application.Domain.Recipe", b =>
                {
                    b.Navigation("Ingredients");

                    b.Navigation("RecipePlanOfMeals");
                });
#pragma warning restore 612, 618
        }
    }
}
