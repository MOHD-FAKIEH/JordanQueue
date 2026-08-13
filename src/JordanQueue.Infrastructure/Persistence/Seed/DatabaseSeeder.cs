using System.Security.Cryptography;
using System.Text;
using JordanQueue.Domain.Constants;
using JordanQueue.Domain.Entities;
using JordanQueue.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JordanQueue.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await context.RolesSet.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded.");
            return;
        }

        logger.LogInformation("Seeding database...");

        var roles = CreateRoles();
        await context.RolesSet.AddRangeAsync(roles, cancellationToken);

        var users = CreateUsers(out var roleAssignments);
        await context.UsersSet.AddRangeAsync(users, cancellationToken);

        foreach (var assignment in roleAssignments)
        {
            await context.UserRolesSet.AddAsync(assignment, cancellationToken);
        }

        var businesses = CreateBusinesses(users, out var services, out var workingHours, out var staff);
        await context.BusinessesSet.AddRangeAsync(businesses, cancellationToken);
        await context.ServicesSet.AddRangeAsync(services, cancellationToken);
        await context.BusinessWorkingHoursSet.AddRangeAsync(workingHours, cancellationToken);
        await context.BusinessStaffSet.AddRangeAsync(staff, cancellationToken);

        var queues = CreateQueues(businesses, services, out var tickets, out var notifications);
        await context.QueuesSet.AddRangeAsync(queues, cancellationToken);
        await context.QueueTicketsSet.AddRangeAsync(tickets, cancellationToken);
        await context.NotificationsSet.AddRangeAsync(notifications, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Database seeding completed.");
    }

    private static List<Role> CreateRoles() =>
    [
        new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111101"), Name = RoleNames.Customer, Description = "Customer role" },
        new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111102"), Name = RoleNames.BusinessOwner, Description = "Business owner role" },
        new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111103"), Name = RoleNames.Staff, Description = "Staff role" },
        new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111104"), Name = RoleNames.SystemAdmin, Description = "System administrator role" }
    ];

    private static List<User> CreateUsers(out List<UserRole> roleAssignments)
    {
        var now = DateTime.UtcNow;
        roleAssignments = [];

        var admin = CreateUser(
            Guid.Parse("22222222-2222-2222-2222-222222222201"),
            "System", "Admin", "+962790000001", "admin@jordanqueue.dev", "Admin123!", now);
        roleAssignments.Add(new UserRole { UserId = admin.Id, RoleId = Guid.Parse("11111111-1111-1111-1111-111111111104") });

        var owner1 = CreateUser(
            Guid.Parse("22222222-2222-2222-2222-222222222202"),
            "Ahmad", "Khalil", "+962790000002", "owner1@jordanqueue.dev", "Owner123!", now);
        roleAssignments.Add(new UserRole { UserId = owner1.Id, RoleId = Guid.Parse("11111111-1111-1111-1111-111111111102") });

        var owner2 = CreateUser(
            Guid.Parse("22222222-2222-2222-2222-222222222203"),
            "Sara", "Nasser", "+962790000003", "owner2@jordanqueue.dev", "Owner123!", now);
        roleAssignments.Add(new UserRole { UserId = owner2.Id, RoleId = Guid.Parse("11111111-1111-1111-1111-111111111102") });

        var staff1 = CreateUser(
            Guid.Parse("22222222-2222-2222-2222-222222222204"),
            "Omar", "Haddad", "+962790000004", "staff1@jordanqueue.dev", "Staff123!", now);
        roleAssignments.Add(new UserRole { UserId = staff1.Id, RoleId = Guid.Parse("11111111-1111-1111-1111-111111111103") });

        var staff2 = CreateUser(
            Guid.Parse("22222222-2222-2222-2222-222222222205"),
            "Layla", "Mansour", "+962790000005", "staff2@jordanqueue.dev", "Staff123!", now);
        roleAssignments.Add(new UserRole { UserId = staff2.Id, RoleId = Guid.Parse("11111111-1111-1111-1111-111111111103") });

        var customers = new List<User>
        {
            CreateUser(Guid.Parse("22222222-2222-2222-2222-222222222206"), "Mohammad", "Ali", "+962790000006", "customer1@jordanqueue.dev", "Customer123!", now),
            CreateUser(Guid.Parse("22222222-2222-2222-2222-222222222207"), "Fatima", "Yousef", "+962790000007", "customer2@jordanqueue.dev", "Customer123!", now),
            CreateUser(Guid.Parse("22222222-2222-2222-2222-222222222208"), "Khaled", "Issa", "+962790000008", "customer3@jordanqueue.dev", "Customer123!", now),
            CreateUser(Guid.Parse("22222222-2222-2222-2222-222222222209"), "Nour", "Saleh", "+962790000009", "customer4@jordanqueue.dev", "Customer123!", now),
            CreateUser(Guid.Parse("22222222-2222-2222-2222-222222222210"), "Yousef", "Hamdan", "+962790000010", "customer5@jordanqueue.dev", "Customer123!", now)
        };

        foreach (var customer in customers)
        {
            roleAssignments.Add(new UserRole { UserId = customer.Id, RoleId = Guid.Parse("11111111-1111-1111-1111-111111111101") });
        }

        var users = new List<User> { admin, owner1, owner2, staff1, staff2 };
        users.AddRange(customers);
        return users;
    }

    private static User CreateUser(Guid id, string firstName, string lastName, string mobile, string email, string password, DateTime now) =>
        new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            MobileNumber = mobile,
            Email = email,
            PasswordHash = HashPassword(password),
            PreferredLanguage = PreferredLanguage.Arabic,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

    private static List<Business> CreateBusinesses(
        List<User> users,
        out List<Service> services,
        out List<BusinessWorkingHours> workingHours,
        out List<BusinessStaff> staff)
    {
        var now = DateTime.UtcNow;
        var owner1 = users.First(u => u.Email == "owner1@jordanqueue.dev");
        var owner2 = users.First(u => u.Email == "owner2@jordanqueue.dev");
        var staff1 = users.First(u => u.Email == "staff1@jordanqueue.dev");
        var staff2 = users.First(u => u.Email == "staff2@jordanqueue.dev");

        var businesses = new List<Business>
        {
            CreateBusiness(Guid.Parse("33333333-3333-3333-3333-333333333301"), owner1.Id, "عيادة النور الطبية", "Al Noor Medical Clinic", BusinessCategory.Clinics, now),
            CreateBusiness(Guid.Parse("33333333-3333-3333-3333-333333333302"), owner1.Id, "حلاق عمان", "Amman Barber", BusinessCategory.Barbers, now),
            CreateBusiness(Guid.Parse("33333333-3333-3333-3333-333333333303"), owner2.Id, "خدمة السيارات السريعة", "Fast Auto Service", BusinessCategory.CarServices, now),
            CreateBusiness(Guid.Parse("33333333-3333-3333-3333-333333333304"), owner2.Id, "Jordan Car Care", "Jordan Car Care", BusinessCategory.CarServices, now),
            CreateBusiness(Guid.Parse("33333333-3333-3333-3333-333333333305"), owner2.Id, "عيادة المدينة للأسنان", "Al Madina Dental Clinic", BusinessCategory.Clinics, now)
        };

        services =
        [
            CreateService(Guid.Parse("44444444-4444-4444-4444-444444444401"), businesses[0].Id, "استشارة", "Consultation", 15),
            CreateService(Guid.Parse("44444444-4444-4444-4444-444444444402"), businesses[1].Id, "قص شعر", "Haircut", 20),
            CreateService(Guid.Parse("44444444-4444-4444-4444-444444444403"), businesses[2].Id, "تغيير زيت", "Oil Change", 30),
            CreateService(Guid.Parse("44444444-4444-4444-4444-444444444404"), businesses[3].Id, "فحص شامل", "Full Inspection", 45),
            CreateService(Guid.Parse("44444444-4444-4444-4444-444444444405"), businesses[4].Id, "فحص أسنان", "Dental Checkup", 25)
        ];

        workingHours = [];
        foreach (var business in businesses)
        {
            for (var day = DayOfWeek.Sunday; day <= DayOfWeek.Thursday; day++)
            {
                workingHours.Add(new BusinessWorkingHours
                {
                    Id = Guid.NewGuid(),
                    BusinessId = business.Id,
                    DayOfWeek = day,
                    OpeningTime = new TimeOnly(9, 0),
                    ClosingTime = new TimeOnly(17, 0),
                    IsClosed = false
                });
            }

            workingHours.Add(new BusinessWorkingHours
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                DayOfWeek = DayOfWeek.Friday,
                OpeningTime = TimeOnly.MinValue,
                ClosingTime = TimeOnly.MinValue,
                IsClosed = true
            });

            workingHours.Add(new BusinessWorkingHours
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                DayOfWeek = DayOfWeek.Saturday,
                OpeningTime = new TimeOnly(10, 0),
                ClosingTime = new TimeOnly(14, 0),
                IsClosed = false
            });
        }

        staff =
        [
            new BusinessStaff { Id = Guid.NewGuid(), BusinessId = businesses[0].Id, UserId = staff1.Id, IsActive = true, CreatedAt = now },
            new BusinessStaff { Id = Guid.NewGuid(), BusinessId = businesses[1].Id, UserId = staff1.Id, IsActive = true, CreatedAt = now },
            new BusinessStaff { Id = Guid.NewGuid(), BusinessId = businesses[2].Id, UserId = staff2.Id, IsActive = true, CreatedAt = now },
            new BusinessStaff { Id = Guid.NewGuid(), BusinessId = businesses[3].Id, UserId = staff2.Id, IsActive = true, CreatedAt = now }
        ];

        return businesses;
    }

    private static Business CreateBusiness(Guid id, Guid ownerId, string nameAr, string nameEn, BusinessCategory category, DateTime now) =>
        new()
        {
            Id = id,
            OwnerUserId = ownerId,
            NameArabic = nameAr,
            NameEnglish = nameEn,
            DescriptionArabic = $"وصف {nameAr}",
            DescriptionEnglish = $"{nameEn} description",
            PhoneNumber = "+962790000000",
            AddressArabic = "عمان، الأردن",
            AddressEnglish = "Amman, Jordan",
            Category = category,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

    private static Service CreateService(Guid id, Guid businessId, string nameAr, string nameEn, int minutes) =>
        new()
        {
            Id = id,
            BusinessId = businessId,
            NameArabic = nameAr,
            NameEnglish = nameEn,
            DescriptionArabic = nameAr,
            DescriptionEnglish = nameEn,
            AverageServiceMinutes = minutes,
            IsActive = true
        };

    private static List<Queue> CreateQueues(
        List<Business> businesses,
        List<Service> services,
        out List<QueueTicket> tickets,
        out List<Notification> notifications)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.FindSystemTimeZoneById("Asia/Amman")));

        var queue = new Queue
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555501"),
            BusinessId = businesses[1].Id,
            ServiceId = services[1].Id,
            QueueDate = today,
            Status = QueueStatus.Open,
            CurrentTicketNumber = 3,
            CreatedAt = now
        };

        tickets =
        [
            new QueueTicket
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666601"),
                QueueId = queue.Id,
                CustomerId = Guid.Parse("22222222-2222-2222-2222-222222222206"),
                TicketNumber = "A001",
                Status = TicketStatus.Served,
                JoinedAt = now.AddHours(-2),
                CalledAt = now.AddHours(-1.5),
                ServedAt = now.AddHours(-1),
                EstimatedWaitMinutes = 0
            },
            new QueueTicket
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666602"),
                QueueId = queue.Id,
                CustomerId = Guid.Parse("22222222-2222-2222-2222-222222222207"),
                TicketNumber = "A002",
                Status = TicketStatus.Called,
                JoinedAt = now.AddHours(-1),
                CalledAt = now.AddMinutes(-10),
                EstimatedWaitMinutes = 20
            },
            new QueueTicket
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666603"),
                QueueId = queue.Id,
                CustomerId = Guid.Parse("22222222-2222-2222-2222-222222222208"),
                TicketNumber = "A003",
                Status = TicketStatus.Waiting,
                JoinedAt = now.AddMinutes(-30),
                EstimatedWaitMinutes = 40
            }
        ];

        notifications =
        [
            new Notification
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222207"),
                TicketId = tickets[1].Id,
                NotificationType = NotificationType.TurnNow,
                Title = "Your turn!",
                Message = "Please proceed to Amman Barber.",
                IsRead = false,
                CreatedAt = now.AddMinutes(-10)
            }
        ];

        return [queue];
    }

    public static string HashPassword(string password)
    {
        var salt = Encoding.UTF8.GetBytes("JordanQueue.Dev.Salt.v1");
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyLegacyPassword(string password, string passwordHash) =>
        HashPassword(password) == passwordHash;
}
