// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OMA.Core.Data;


#nullable disable

namespace OMA.Migrations
{
    [DbContext(typeof(OMAContext))]
    partial class OMAContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("AliasLobby", b =>
                {
                    b.Property<int>("AliasesId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LobbiesId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AliasesId", "LobbiesId");

                    b.HasIndex("LobbiesId");

                    b.ToTable("AliasLobby");
                });

            modelBuilder.Entity("OMA.Models.Alias", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Aliases");
                });

            modelBuilder.Entity("OMA.Models.Lobby", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BestOf")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LobbyId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LobbyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Warmups")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Lobbies");
                });

            modelBuilder.Entity("AliasLobby", b =>
                {
                    b.HasOne("OMA.Models.Alias", null)
                        .WithMany()
                        .HasForeignKey("AliasesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OMA.Models.Lobby", null)
                        .WithMany()
                        .HasForeignKey("LobbiesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
