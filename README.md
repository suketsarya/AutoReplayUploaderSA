# AutoReplayUploaderSA `v0.1.0`

A modern, standalone desktop application for Rocket League players to automatically track and upload their replays to **Ballchasing.com**. Built with .NET 10.0 and Blazor Hybrid (WPF), it offers a premium, high-performance experience without the need for BakkesMod.

## Key Features

- **Real-Time Monitoring**: Automatically detects new replays the moment they are saved in yor Rocket League `Demos` folder.
- **Intelligent Startup**: Uses a dynamic baseline to ignore old replays on launch. It only shows you what's relevant now.
- **Auto-Delete Workflow**: Keep your local folders clean! Automatically deletes local `.replay` files a specified number of minutes after a successful upload.
- **Dual-Tab Interface**: Separate your active work from your history with dedicated **Pending & Failed** and **Uploaded** tabs.
- **Bulk Imports**: Need to upload an old folder? Use the "Add Replay File(s)..." feature to manually import batches of replays, bypassing the standard date filters.
- **Quota Awareness**: Real-time tracking of your Ballchasing.com daily and weekly upload limits. Buttons automatically disable when your quota is reached to prevent redundant errors.
- **Modern UI**: A sleek, dark-mode interface featuring glassmorphism, sticky controls, and smooth animations.

## Configuration & Settings

You can customize the following settings to match your workflow:

### **API Configuration**
- **Ballchasing API Key**: Required to connect to your account. Your key is stored securely in your local application data.
- **Default Visibility**: Set your preferred default for new uploads (**Public**, **Private**, or **Unlisted**). You can still override this for individual replays using the dropdown menu.

### **Monitoring & Automation**
- **Replay Folder**: Point the app to your Rocket League replay directory (usually `%USERPROFILE%\Documents\My Games\Rocket League\TAGame\Demos`).
- **Polling Interval**: Adjust how often the app checks for new files (in seconds).
- **Auto-Upload Toggle**: Enable or disable background uploads globally.

### **Cleanup (Auto-Delete)**
- **Enable Auto-Delete**: When turned on, the app will remove the local `.replay` file from your disk after it is successfully uploaded.
- **Cleanup Delay**: Specify how many minutes to wait after a successful upload before deleting the file (useful if you want to keep files briefly for local viewing).

## How to Use

1. **Get your API Key**: Head to [Ballchasing.com/upload](https://ballchasing.com/upload) and copy your API Key.
2. **Setup**: Open **Settings** in the app, paste your key, and select your replay folder (defaulted to My Games/.../Demos).
3. **Play**: Just keep the app running while you play Rocket League. Your replays will appear in the "Pending" tab and move to "Uploaded" automatically.
4. **View Online**: Click the **"View Replay"** button on any successfully uploaded item to jump straight to the Ballchasing analysis page.

## Installation

This application is distributed as a convenient installer.
1. Download the `AutoReplayUploaderSA_Setup.exe` from the [Releases](https://github.com/YOUR_USERNAME/Suket-AutoReplayUploader/releases) page.
2. Run the installer to automatically set up the application and its dependencies on your machine.
3. The installer includes an option to create a desktop shortcut for easy access.

## Contribution

Feel free to fork, raise a PR. If you have any feature requests, open an issue and I'll look into it. Also, feel free to open an issue for any bugs you find.

---
*Note: This application is not affiliated with Psyonix or Ballchasing.com. It is an independent tool built for the Rocket League community.*