using Avalonia.Controls;
using Henson.Models;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Tomlyn;

namespace Henson.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Settings = LoadSettings();
            SetSettings();

            DbClient.CreateDbIfNotExists();

            RxApp.MainThreadScheduler.Schedule(LoadNations);

            AddNationCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;
                var dialog = new AddNationWindowViewModel();
                var result = await AddNationDialog.Handle(dialog);

                if(result != null)
                {
                    FooterText = "Loading nations... this may take a while.";
                    await Task.Delay(100);

                    var (nations, authFailedOnSome) = Client.AuthenticateAndReturnInfo(result);
                    foreach(Nation n in nations)
                    {
                        if(Nations.Select(x => x.Name).Contains(n.Name)) continue;

                        Nations.Add(new NationGridViewModel(n, true, this));
                        DbClient.InsertNation(n);
                    }

                    if(authFailedOnSome)
                    {
                        var messageBoxDialog = new MessageBoxViewModel(new MessageBoxStandardParams
                        {
                            ContentTitle = "Warning",
                            ContentMessage = "One or more nation(s) failed to add, probably due to an invalid username/password combo.",
                            Icon = Icon.Warning,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        });
                        await MessageBoxDialog.Handle(messageBoxDialog);
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
                    FooterText = "Finished loading!";
                }
            });

            RemoveSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                {
                    ContentTitle = "Remove Selected Nations",
                    ContentMessage = "Are you sure you want to remove the selected nations' logins from Henson?",
                    Icon = Icon.Info,
                    ButtonDefinitions = ButtonEnum.YesNo,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                var result = await MessageBoxDialog.Handle(dialog);

                if(result == ButtonResult.Yes)
                {
                    for(int i = Nations.Count - 1; i >= 0; i--)
                    {
                        if(Nations[i].Checked)
                        {
                            DbClient.DeleteNation(Nations[i].Name);
                            Nations.RemoveAt(i);
                        }
                    }

                    FooterText = "Nations removed!";
                }
            });

            PingSelectedCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;
                
                var selectedNations = Nations.Where(x => x.Checked).ToList();
                var nationLogins = selectedNations.Select(x => new NationLoginViewModel(x.Name, x.Pass)).ToList();

                FooterText = "Pinging nations...";
                await Task.Delay(100);

                var successes = Client.PingMany(nationLogins);
                if(!successes.All(x => x))
                {
                    for(int i = 0; i < selectedNations.Count; i++)
                    {
                        selectedNations[i].Checked = successes[i];
                    }
                    
                    FooterText = "Nations pinged (some failed)!";

                    var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "Warning",
                        ContentMessage = "One or more nation(s) failed to ping, probably due to an invalid username/password combo. They have been selected.",
                        Icon = Icon.Warning,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    });
                    await MessageBoxDialog.Handle(dialog);
                }
                else
                {
                    FooterText = "Nations pinged!";

                    var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "Success",
                        ContentMessage = "All nations pinged successfully.",
                        Icon = Icon.Info,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    });
                    await MessageBoxDialog.Handle(dialog);
                }
            });

            FindWACommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if(await UserAgentNotSet()) return;
                FooterText = "Finding WA nation...";
                await Task.Delay(100);

                var result = Client.FindWA(Nations.ToList());

                if(result != null)
                {
                    FooterText = $"WA nation found: {result}";

                    var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "WA Nation Found",
                        ContentMessage = $"Your WA nation is {result}.",
                        Icon = Icon.Info,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    });
                    await MessageBoxDialog.Handle(dialog);
                }
                else
                {
                    FooterText = $"WA nation not found.";

                    var dialog = new MessageBoxViewModel(new MessageBoxStandardParams
                    {
                        ContentTitle = "WA Nation Not Found",
                        ContentMessage = "Your WA nation was not found.",
                        Icon = Icon.Warning,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    });
                    await MessageBoxDialog.Handle(dialog);
                }
            });
        }

        private void LoadNations()
        {
            var nations = DbClient.GetNations();

            foreach(var n in nations)
            {
                Nations.Add(new NationGridViewModel(n, false, this));
            }
        }

        private static ProgramSettingsViewModel LoadSettings()
        {
            var workingPath = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            var path = Path.Combine(workingPath, "settings.toml");

            if(!File.Exists(path))
            {
                File.WriteAllText(path, "user_agent = \"\"");
            }

            string setTomlString = File.ReadAllText(path);
            return Toml.ToModel<ProgramSettingsViewModel>(setTomlString); //This will work for now-later, find solution for interversion compatibility
        }

        private void SetSettings()
        {
            Client.UserAgent = Settings.UserAgent;
        }

        public ObservableCollection<NationGridViewModel> Nations { get; } = new();
        public ProgramSettingsViewModel Settings { get; set; }
        public NsClient Client { get; } = new();

        private string currentLoginUser = "";
        public string CurrentLoginUser
        {
            get => currentLoginUser;
            set => this.RaiseAndSetIfChanged(ref currentLoginUser, value);
        }

        private string? currentLocalID = null;

        private string footerText = "Welcome to Henson!";
        public string FooterText
        {
            get => footerText;
            set => this.RaiseAndSetIfChanged(ref footerText, value);
        }
        
        public ICommand AddNationCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand PingSelectedCommand { get; }
        public ICommand FindWACommand { get; }

        public Interaction<AddNationWindowViewModel, List<NationLoginViewModel>?> AddNationDialog { get; } = new(); //I will simplify all of this eventually
        public Interaction<MessageBoxViewModel, ButtonResult> MessageBoxDialog { get; } = new();

        public void OnSelectNationsClick()
        {
            bool OppositeAllTrueOrFalse = !Nations.All(x => x.Checked);
            foreach(var nation in Nations)
            {
                nation.Checked = OppositeAllTrueOrFalse;
            }
        }

        public void OnSaveSettingsClick()
        {
            var workingPath = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            var path = Path.Combine(workingPath, "settings.toml");
            File.WriteAllText(path, Toml.FromModel(Settings));

            SetSettings();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) SystemSounds.Beep.Play();
            FooterText = $"Settings updated.";
        }

        public async Task OnNationLoginClick(NationGridViewModel nation)
        {
            if(await UserAgentNotSet()) return;
            var nationLogin = new NationLoginViewModel(nation.Name, nation.Pass);

            FooterText = $"Logging in to {nation.Name}...";
            await Task.Delay(100);

            var result = Client.Login(nationLogin);
            if (result != null)
            {
                nation.Chk = result;

                CurrentLoginUser = nation.Name;
                currentLocalID = null;
                FooterText = $"Logged in to {nation.Name}";
            }
            else
            {
                FooterText = $"Failed to log in to {nation.Name}";
                await Task.Delay(100);

                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Login Failed",
                    ContentMessage = "The login failed, probably due to an invalid username/password combination.",
                    Icon = Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                await MessageBoxDialog.Handle(dialog);
            }
        }

        private async Task<bool> NationEqualsLogin(NationGridViewModel nation)
        {
            if(nation.Name != currentLoginUser)
            {
                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Current Login Doesn't Match",
                    ContentMessage = "Please log in with the the account you are trying to perform this action with.",
                    Icon = Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                await MessageBoxDialog.Handle(dialog);
                return false;
            }
            return true;
        }

        private async Task<bool> UserAgentNotSet()
        {
            if(Settings.UserAgent == "")
            {
                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "User Agent Not Set",
                    ContentMessage = "Please go to the Settings tab to set the User Agent.",
                    Icon = Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                await MessageBoxDialog.Handle(dialog);
                return true;
            }
            return false;
        }

        public async Task OnNationApplyWAClick(NationGridViewModel nation)
        {
            if(await UserAgentNotSet()) return;
            if(!await NationEqualsLogin(nation)) return;

            var chk = nation.Chk!;

            if(Client.ApplyWA(chk))
            {
                FooterText = $"{nation.Name} WA application successful!";
            }
            else
            {
                CurrentLoginUser = "";
                currentLocalID = null;
                FooterText = $"{nation.Name} WA application failed... please log in again.";
                await Task.Delay(100);

                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "WA Application Failed",
                    ContentMessage = "Please log in again.",
                    Icon = Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                await MessageBoxDialog.Handle(dialog);
            }
        }

        public async Task OnNationGetLocalIDClick(NationGridViewModel nation)
        {
            if(await UserAgentNotSet()) return;
            if(!await NationEqualsLogin(nation)) return;

            var localID = Client.GetLocalID();

            if(localID != null)
            {
                currentLocalID = localID;
                FooterText = $"Got local ID of {nation.Name}, ready to move regions!";
            }
            else
            {
                CurrentLoginUser = "";
                currentLocalID = null;
                FooterText = $"Getting local ID failed... please log in again.";
                await Task.Delay(100);

                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Local ID Not Found",
                    ContentMessage = "Please log in again.",
                    Icon = Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                await MessageBoxDialog.Handle(dialog);
            }
        }

        public async Task OnNationMoveRegionClick(NationGridViewModel nation, string region)
        {
            if(await UserAgentNotSet()) return;
            if(!await NationEqualsLogin(nation)) return;

            if(region == null)
            {
                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Target Region Not Set",
                    ContentMessage = "Please set a target region.",
                    Icon = Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                await MessageBoxDialog.Handle(dialog);
                return;
            }
            
            if(currentLocalID == null)
            {
                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Local ID Needed",
                    ContentMessage = "Please get the local ID before jumping region.",
                    Icon = Icon.Warning,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                await MessageBoxDialog.Handle(dialog);
                return;
            }

            FooterText = $"Moving {nation.Name} to {region}... this may take a while.";
            await Task.Delay(100);

            if (Client.MoveToJP(region, currentLocalID))
            {
                FooterText = $"{nation.Name} moved to {region}!";
                nation.Region = char.ToUpper(region[0]) + region[1..];
                DbClient.ExecuteNonQuery($"UPDATE nations SET region = '{region}' WHERE name = '{nation.Name}'");
            }
            else
            {
                FooterText = $"Moving {nation.Name} to region {region} failed!";
                await Task.Delay(100);

                MessageBoxViewModel dialog = new(new MessageBoxStandardParams
                {
                    ContentTitle = "Moving region failed",
                    ContentMessage = "Moving to the region failed.",
                    Icon = Icon.Error,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                });
                await MessageBoxDialog.Handle(dialog);
            }
        }
    }
}
