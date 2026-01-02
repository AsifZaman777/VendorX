# VendorX - Vendor/Shop Management System

A comprehensive vendor and shop management system built with ASP.NET Core MVC (.NET 10), featuring role-based access for SuperAdmin, ShopKeeper, and Customer roles.

## Features

### SuperAdmin Features
- **Shop Management**: Create, read, update, and delete shops
- **Customer Management**: Manage customers across all shops
- Full system oversight and administration

### ShopKeeper Features
- **POS System**: Process instant purchases with automatic notifications
- **Order Management**: Create pending orders and manage delivery
- **Product & Category Management**: Organize inventory by categories
- **Baki Management**: Track credit purchases (Baki)
  - Create, settle, and manage Baki records
  - Generate monthly invoices for customers
  - Send invoices via email/WhatsApp
- **Customer Management**: Register and manage shop customers
- **Reports**:
  - Daily sales report
  - Baki report (credit history)
  - Expense report
  - Profit/Expense analysis

### Customer Features
- **Shop Registration**: Scan QR code to register to shops
- **Purchase History**: View all instant purchases from shops
- **Order Tracking**: Monitor pending, delivered, and cancelled orders
- **Baki Management**:
  - View Baki history across all shops
  - Check Baki summary by shop
  - Receive and view monthly invoices

## Technology Stack

- **.NET 10** - Latest .NET framework
- **ASP.NET Core MVC** - Web application framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Database
- **ASP.NET Core Identity** - Authentication & authorization
- **MailKit** - Email notifications
- **QRCoder** - QR code generation

## Getting Started

### Prerequisites

- Visual Studio 2022 (17.12 or later) with .NET 10 SDK
- SQL Server (LocalDB or full instance)
- .NET 10 SDK

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd VendorX
   ```

2. **Update Database Connection String**
   
   Open `appsettings.json` and update the connection string if needed:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VendorXDb;Trusted_Connection=true;MultipleActiveResultSets=true"
   }
   ```

3. **Configure Email Settings** (Optional)
   
   Update email settings in `appsettings.json`:
   ```json
   "EmailSettings": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": 587,
     "SenderEmail": "your-email@gmail.com",
     "SenderName": "VendorX",
     "Username": "your-email@gmail.com",
     "Password": "your-app-password"
   }
   ```

4. **Create Database Migration**
   
   Open Package Manager Console in Visual Studio and run:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```

5. **Run the Application**
   
   Press `F5` in Visual Studio or:
   ```bash
   dotnet run
   ```

### Default Credentials

After running the application for the first time, a SuperAdmin account will be automatically created:

- **Email**: admin@vendorx.com
- **Password**: Admin@123

**Important**: Change this password after first login!

## Usage

### For SuperAdmin

1. Login with SuperAdmin credentials
2. Navigate to SuperAdmin area
3. Create shops and assign them to ShopKeeper users
4. Manage customers across the system

### For ShopKeeper

1. Register as ShopKeeper or login
2. Set up your shop (categories, products)
3. Register customers or have them scan your shop's QR code
4. Process sales through POS
5. Create orders for later delivery
6. Manage Baki (credit purchases)
7. Generate and send monthly invoices
8. View reports

### For Customers

1. Register as Customer
2. Scan shop QR code to register to a shop
3. Make purchases (instant or orders)
4. View purchase history
5. Track Baki across shops
6. Receive notifications via email/WhatsApp

## Project Structure

```
VendorX/
??? Areas/
?   ??? SuperAdmin/
?   ?   ??? Controllers/
?   ??? ShopKeeper/
?   ?   ??? Controllers/
?   ??? Customer/
?       ??? Controllers/
??? Controllers/
?   ??? AccountController.cs
?   ??? HomeController.cs
??? Data/
?   ??? ApplicationDbContext.cs
?   ??? DbSeeder.cs
??? Models/
?   ??? ApplicationUser.cs
?   ??? Shop.cs
?   ??? Customer.cs
?   ??? Product.cs
?   ??? Category.cs
?   ??? Order.cs
?   ??? POSTransaction.cs
?   ??? Baki.cs
?   ??? BakiInvoice.cs
?   ??? Expense.cs
?   ??? Enums/
??? Services/
?   ??? EmailService.cs
?   ??? WhatsAppService.cs
?   ??? QRCodeService.cs
?   ??? ShopService.cs
?   ??? CustomerService.cs
?   ??? BakiService.cs
??? ViewModels/
?   ??? AccountViewModels.cs
?   ??? ShopKeeperViewModels.cs
?   ??? POSViewModels.cs
?   ??? BakiViewModels.cs
?   ??? ReportViewModels.cs
??? Views/
    ??? Shared/
        ??? _Layout.cshtml
```

## Key Concepts

### Baki System

"Baki" refers to credit purchases where customers buy items and settle payment at the end of the month. The system:
- Tracks all Baki transactions
- Allows shopkeepers to mark Baki as settled
- Generates monthly invoices
- Sends notifications to customers

### QR Code Registration

Each shop gets a unique QR code during creation. Customers can:
1. Scan the QR code using any QR reader
2. Copy the code (format: `SHOP:XXXXXXXX`)
3. Register to the shop through the customer portal

### Notifications

The system sends notifications via:
- **Email**: Using SMTP (MailKit)
- **WhatsApp**: Integration ready (requires WhatsApp Business API setup)

Notifications are sent for:
- New purchases
- Order status updates
- Baki record creation/settlement
- Monthly invoices

## Database Schema

### Key Tables
- **AspNetUsers** - User accounts with roles
- **Shops** - Shop information with QR codes
- **Customers** - Customer details
- **ShopCustomers** - Many-to-many relationship
- **Products & Categories** - Inventory management
- **Orders & OrderItems** - Pending orders
- **POSTransactions** - Instant purchases
- **Baki** - Credit purchase records
- **BakiInvoices** - Monthly invoices
- **Expenses** - Shop expenses

## API Integration Points

### WhatsApp Integration

The `WhatsAppService.cs` is ready for integration with WhatsApp Business API providers like:
- Twilio
- MessageBird
- WhatsApp Business API

Update the `SendWhatsAppMessageAsync` method with your provider's API.

### Email Integration

Currently configured for Gmail SMTP. For production:
1. Create an App Password in Google Account settings
2. Update `appsettings.json` with your credentials
3. For other providers, update SMTP settings accordingly

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Check connection string in `appsettings.json`
- Verify database exists: Run `Update-Database` in Package Manager Console

### Migration Issues
- Delete `Migrations` folder
- Run `Add-Migration InitialCreate`
- Run `Update-Database`

### Email Not Sending
- Enable "Less secure app access" or use App Passwords for Gmail
- Check firewall settings for SMTP ports
- Verify SMTP credentials

## Future Enhancements

- [ ] Mobile app integration
- [ ] SMS notifications
- [ ] Advanced reporting with charts
- [ ] Multi-currency support
- [ ] Inventory alerts
- [ ] Barcode scanning
- [ ] Payment gateway integration
- [ ] Export reports to PDF/Excel

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions:
- Create an issue in the repository
- Contact: support@vendorx.com

---

**Note**: This is a complete management system. Start by creating shops, adding products, and registering customers. The system handles the rest with automatic notifications and comprehensive reporting.
