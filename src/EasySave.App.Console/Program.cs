// See https://aka.ms/new-console-template for more information
using System;
using EasySave.Core.Enums;
using EasySave.Core.Models;
using EasySave.App.Repositories;    

class Program
{
    static void Main(string[] args)
    {
        JobRepository jobRepository = new JobRepository();
        BackupJob job1 = new BackupJob("job1", "Documents Backup", "C:\\Users\\User\\Documents", "D:\\Backups\\Documents", BackupType.Full);
        BackupJob job2 = new BackupJob("job2", "Pictures Backup", "C:\\Users\\User\\Pictures", "D:\\Backups\\Pictures", BackupType.Differential);
        jobRepository.Add(job1);
        jobRepository.Add(job2);
        var allJobs = jobRepository.GetAll();
        Console.WriteLine("Current Backup Jobs:");
        foreach (var job in allJobs)
        {
            Console.WriteLine($"- {job.Name} ({job.Type})");
        }
        BackupJob updatedJob1 = new BackupJob("job1", "Updated Documents Backup", "C:\\Users\\User\\Documents", "D:\\Backups\\Docs", BackupType.Differential);
        jobRepository.Update(updatedJob1);

        jobRepository.Remove("job2");
        Console.WriteLine("After updating job1 and removing job2:");

        foreach (var job in allJobs)
        {
            Console.WriteLine($"- {job.Name} ({job.Type}) | Source: {job.SourcePath} -> Target: {job.TargetPath}");
        }
    }
}