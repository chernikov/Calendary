﻿// <auto-generated />
using System;
using Calendary.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Calendary.Repos.Migrations
{
    [DbContext(typeof(CalendaryDbContext))]
    [Migration("20241020091049_RemoveCalendarIdInHolidayAddLanguageCode")]
    partial class RemoveCalendarIdInHolidayAddLanguageCode
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Calendary.Model.Calendar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CountryId")
                        .HasColumnType("int");

                    b.Property<string>("FilePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FirstDayOfWeek")
                        .HasColumnType("int");

                    b.Property<bool>("IsCurrent")
                        .HasColumnType("bit");

                    b.Property<int>("LanguageId")
                        .HasColumnType("int");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.HasIndex("LanguageId");

                    b.HasIndex("OrderId");

                    b.ToTable("Calendars");
                });

            modelBuilder.Entity("Calendary.Model.CalendarHoliday", b =>
                {
                    b.Property<int>("CalendarId")
                        .HasColumnType("int");

                    b.Property<int>("HolidayId")
                        .HasColumnType("int");

                    b.HasKey("CalendarId", "HolidayId");

                    b.HasIndex("HolidayId");

                    b.ToTable("CalendarHolidays");
                });

            modelBuilder.Entity("Calendary.Model.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Countries");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Code = "UA",
                            Name = "Україна"
                        });
                });

            modelBuilder.Entity("Calendary.Model.EventDate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CalendarId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserSettingId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CalendarId");

                    b.HasIndex("UserSettingId");

                    b.ToTable("EventDates");
                });

            modelBuilder.Entity("Calendary.Model.Holiday", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CountryId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.ToTable("Holidays");
                });

            modelBuilder.Entity("Calendary.Model.Image", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CalendarId")
                        .HasColumnType("int");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<short>("Order")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.HasIndex("CalendarId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Calendary.Model.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Languages");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Code = "uk-UA",
                            Name = "Українська"
                        },
                        new
                        {
                            Id = 2,
                            Code = "en-EN",
                            Name = "English"
                        });
                });

            modelBuilder.Entity("Calendary.Model.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DeliveryAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsPaid")
                        .HasColumnType("bit");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("Calendary.Model.PaymentInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsPaid")
                        .HasColumnType("bit");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("OrderId")
                        .IsUnique();

                    b.ToTable("PaymentInfos");
                });

            modelBuilder.Entity("Calendary.Model.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Admin"
                        },
                        new
                        {
                            Id = 2,
                            Name = "User"
                        });
                });

            modelBuilder.Entity("Calendary.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsEmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Email = "admin@calendary.com.ua",
                            IsEmailConfirmed = true,
                            IsPhoneNumberConfirmed = true,
                            PasswordHash = "21232f297a57a5a743894a0e4a801fc3",
                            UserName = "admin"
                        });
                });

            modelBuilder.Entity("Calendary.Model.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");

                    b.HasData(
                        new
                        {
                            UserId = 1,
                            RoleId = 1
                        });
                });

            modelBuilder.Entity("Calendary.Model.UserSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CountryId")
                        .HasColumnType("int");

                    b.Property<int>("FirstDayOfWeek")
                        .HasColumnType("int");

                    b.Property<int>("LanguageId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.HasIndex("LanguageId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("UserSettings");
                });

            modelBuilder.Entity("Calendary.Model.Calendar", b =>
                {
                    b.HasOne("Calendary.Model.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Calendary.Model.Language", "Language")
                        .WithMany()
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Calendary.Model.Order", "Order")
                        .WithMany("Calendars")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");

                    b.Navigation("Language");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Calendary.Model.CalendarHoliday", b =>
                {
                    b.HasOne("Calendary.Model.Calendar", "Calendar")
                        .WithMany("CalendarHolidays")
                        .HasForeignKey("CalendarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Calendary.Model.Holiday", "Holiday")
                        .WithMany()
                        .HasForeignKey("HolidayId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Calendar");

                    b.Navigation("Holiday");
                });

            modelBuilder.Entity("Calendary.Model.EventDate", b =>
                {
                    b.HasOne("Calendary.Model.Calendar", "Calendar")
                        .WithMany("EventDates")
                        .HasForeignKey("CalendarId");

                    b.HasOne("Calendary.Model.UserSetting", "UserSetting")
                        .WithMany("EventDates")
                        .HasForeignKey("UserSettingId");

                    b.Navigation("Calendar");

                    b.Navigation("UserSetting");
                });

            modelBuilder.Entity("Calendary.Model.Holiday", b =>
                {
                    b.HasOne("Calendary.Model.Country", "Country")
                        .WithMany("Holidays")
                        .HasForeignKey("CountryId");

                    b.Navigation("Country");
                });

            modelBuilder.Entity("Calendary.Model.Image", b =>
                {
                    b.HasOne("Calendary.Model.Calendar", "Calendar")
                        .WithMany("Images")
                        .HasForeignKey("CalendarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Calendar");
                });

            modelBuilder.Entity("Calendary.Model.Order", b =>
                {
                    b.HasOne("Calendary.Model.User", "User")
                        .WithMany("Orders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Calendary.Model.PaymentInfo", b =>
                {
                    b.HasOne("Calendary.Model.Order", "Order")
                        .WithOne("PaymentInfo")
                        .HasForeignKey("Calendary.Model.PaymentInfo", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("Calendary.Model.UserRole", b =>
                {
                    b.HasOne("Calendary.Model.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Calendary.Model.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Calendary.Model.UserSetting", b =>
                {
                    b.HasOne("Calendary.Model.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Calendary.Model.Language", "Language")
                        .WithMany()
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Calendary.Model.User", "User")
                        .WithOne("Settings")
                        .HasForeignKey("Calendary.Model.UserSetting", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");

                    b.Navigation("Language");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Calendary.Model.Calendar", b =>
                {
                    b.Navigation("CalendarHolidays");

                    b.Navigation("EventDates");

                    b.Navigation("Images");
                });

            modelBuilder.Entity("Calendary.Model.Country", b =>
                {
                    b.Navigation("Holidays");
                });

            modelBuilder.Entity("Calendary.Model.Order", b =>
                {
                    b.Navigation("Calendars");

                    b.Navigation("PaymentInfo")
                        .IsRequired();
                });

            modelBuilder.Entity("Calendary.Model.User", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("Settings")
                        .IsRequired();

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("Calendary.Model.UserSetting", b =>
                {
                    b.Navigation("EventDates");
                });
#pragma warning restore 612, 618
        }
    }
}
