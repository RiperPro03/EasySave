using System;
using System.Diagnostics;
// Donne accès à Stopwatch
// Utilisé dans notre cas pour mesurer le temps de copie de chaque fichier
using System.IO;
// Bibliothèque dédiée à la gestion du système de fichiers, elle permet d'utiliser Directory, File et Path
using EasySave.Core.Interfaces;
// Là où nous avons notre l’interface IBackupService
    
namespace EasySave.App.Services;

public class BackupService : IBackupService
{
    public void FullBackup(string sourcePath, string targetPath)
    {
        try
        {

            // Vérification du dossier source
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"[ERREUR] Le dossier source n'existe pas : {sourcePath}");
                return;
            }

            // Création du dossier cible s'il n'existe pas
            Directory.CreateDirectory(targetPath);

            CopyDirectory(sourcePath, targetPath);
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    private void CopyDirectory(string sourceDir, string targetDir)
    {
        // Copie des fichiers
        // Directory.GetFiles retourne tous les fichiers du dossier courant mais ne va pas dans les sous-dossiers
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            try
            {   
                // Extrait le nom du fichier
                string fileName = Path.GetFileName(file);
                // Construit un chemin valide (Windows/Linux)
                string targetFilePath = Path.Combine(targetDir, fileName);
                
                // Démarre le chronomètre
                var stopwatch = Stopwatch.StartNew();

                File.Copy(file, targetFilePath, true);
                
                stopwatch.Stop();

                Console.WriteLine(
                    $"[OK] {fileName} copié en {stopwatch.ElapsedMilliseconds} ms");
            }
            // Si un fichier échoue, la sauvegarde continue et les autres fichiers sont copiés
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        // Copie récursive des sous-dossiers
        foreach (string directory in Directory.GetDirectories(sourceDir))
        {
            try
            {
                string dirName = Path.GetFileName(directory);
                string targetSubDir = Path.Combine(targetDir, dirName);

                Directory.CreateDirectory(targetSubDir);
                
                // Appel récursif
                // La fonction s'appelle elle même pour traiter les sous dossiers
                CopyDirectory(directory, targetSubDir);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
    }

    private void LogError(Exception ex)
    {
        // Log simple (console pour l’instant)
        Console.WriteLine($"[ERREUR] {ex.Message}");
    }
}

