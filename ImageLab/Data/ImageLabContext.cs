using Microsoft.EntityFrameworkCore;

namespace ImageLab.Data;

public class ImageLabContext : DbContext
{
    public ImageLabContext(DbContextOptions<ImageLabContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}