using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleID = 1)
                BEGIN
                    INSERT INTO Roles (RoleID, RoleName)
                    VALUES (1, 'Learner');
                END;

                IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleID = 2)
                BEGIN
                    INSERT INTO Roles (RoleID, RoleName)
                    VALUES (2, 'Admin');
                END;

                IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleID = 3)
                BEGIN
                    INSERT INTO Roles (RoleID, RoleName)
                    VALUES (3, 'Sponsor');
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Roles are required system data and may already exist
            // independently of this migration.
        }
    }
}