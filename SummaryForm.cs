using HnsExplorer.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HnsExplorer;

public partial class SummaryForm : Form
{
    private HnsDatasource datasource;

    public SummaryForm(HnsDatasource datasource)
    {
        this.datasource = datasource;
        InitializeComponent();
        LoadTreeview();
        textBox1.KeyPress += new KeyPressEventHandler(CheckEnterKeyPress);
        textBox1.Focus();
        textBox1.Select();
    }

    private void LoadTreeview()
    {
        datasource.Reset();
        Task.Run(() =>
        {
            var loadingForm = new SplashForm(datasource);
            if (Visible)
            {
                var x = Location.X + (Width / 2) - (loadingForm.Width / 2);
                var y = Location.Y + (Height / 2) - (loadingForm.Height / 2);
                loadingForm.StartPosition = FormStartPosition.Manual;
                loadingForm.Location = new Point(x, y);
            }
            else
            {
                loadingForm.StartPosition = FormStartPosition.CenterScreen;
            }
            Application.Run(loadingForm);
            loadingForm.Dispose();
        });
        datasource.Load();

        richTextBox1.Text = datasource.SummaryOutput;
        treeView1.Nodes.Clear();
        treeView1.Nodes.Add(datasource.ActivitiesNode);
        treeView1.Nodes.Add(datasource.RoutesNode);

        if (datasource.OrphansNode.Nodes.Count > 0)
        {
            treeView1.Nodes.Add(datasource.OrphansNode);
        }

        if (datasource.ContainerdNode.Nodes.Count > 0)
        {
            treeView1.Nodes.Add(datasource.ContainerdNode);
        }
    }

    private void button1_Click(object sender, EventArgs e)
    {
        LoadTreeview();
        SearchRecursive(treeView1.Nodes, textBox1.Text);
        SelectFirst(treeView1.Nodes, textBox1.Text);
    }

    private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if(e.Node is not null)
        {
            ClearSearchResults(treeView1.Nodes);
            SearchRecursive(treeView1.Nodes, textBox1.Text, false);
            e.Node.ForeColor = Program.ACTIVE_COLOR_HIGHLIGHT_FOREGROUND;
            e.Node.BackColor = Program.ACTIVE_COLOR_HIGHLIGHT_BACKGROUND;

            if (e.Node.Tag is string)
            {
                richTextBox1.Text = e.Node.Tag.ToString();
            }
            else
            {
                richTextBox1.Text = JsonSerializer.Serialize(
                    e.Node.Tag,
                    new JsonSerializerOptions()
                    {
                        WriteIndented = true,
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    });
            }
            HighlightSearchTerms(richTextBox1);
        }
        else
        {
            richTextBox1.Text = "Nothing selected in the tree view";
        }
    }

    private void HighlightSearchTerms(RichTextBox richTextBox)
    {
        if(string.IsNullOrEmpty(textBox1.Text))
        {
            return;
        }
        richTextBox.SelectAll();
        richTextBox.SelectionColor = richTextBox.ForeColor;
        richTextBox.SelectionBackColor = richTextBox.BackColor;
        var searchIndex = richTextBox.Text.IndexOf(textBox1.Text, StringComparison.OrdinalIgnoreCase);
        var firstSearchIndex = searchIndex;
        while (searchIndex > 0)
        {
            richTextBox.Select(searchIndex, textBox1.Text.Length);
            richTextBox.SelectionColor = Program.ACTIVE_COLOR_SEARCH_FOREGROUND;
            richTextBox.SelectionBackColor = Program.ACTIVE_COLOR_SEARCH_HIGHLIGHT;
            if (searchIndex == firstSearchIndex)
            {
                richTextBox.ScrollToCaret();
            }
            searchIndex = richTextBox.Text.IndexOf(textBox1.Text, (searchIndex + textBox1.Text.Length), StringComparison.OrdinalIgnoreCase);
        }
    }

    private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private void SummaryForm_Load(object sender, EventArgs e)
    {

    }

    private void textBox1_TextChanged_1(object sender, EventArgs e)
    {

    }

    private void CheckEnterKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Return)
        {
            ClearSearchResults(treeView1.Nodes);
            SearchRecursive(treeView1.Nodes, textBox1.Text);
            SelectFirst(treeView1.Nodes, textBox1.Text);
            HighlightSearchTerms(richTextBox1);
        }
    }

    private void ClearSearchResults(TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            node.ForeColor = Program.ACTIVE_COLOR_FOREGROUND;
            node.BackColor = Program.ACTIVE_COLOR_BACKGROUND_TEXTBOX;
            ClearSearchResults(node.Nodes);
        }
    }

    private bool SelectFirst(TreeNodeCollection nodes, string searchFor)
    {
        foreach (TreeNode node in nodes)
        {
            var tagText = (node.Tag is not null) ? node.Tag.ToString() ?? string.Empty : string.Empty;
            if (node.Text.Contains(searchFor, StringComparison.OrdinalIgnoreCase) || tagText.Contains(searchFor, StringComparison.OrdinalIgnoreCase))
            {
                treeView1.SelectedNode = node;
                return true;
            }
            else
            {
                if (SelectFirst(node.Nodes, searchFor))
                    return true;
            }
        }
        return false;
    }

    private bool SearchRecursive(TreeNodeCollection nodes, string searchFor, bool expandToMakeVisible = true)
    {
        if (string.IsNullOrEmpty(searchFor))
        {
            return false;
        }
        foreach (TreeNode node in nodes)
        {
            var tagText = (node.Tag is not null) ? node.Tag.ToString() ?? string.Empty : string.Empty;
            if (node.Text.Contains(searchFor, StringComparison.OrdinalIgnoreCase) || tagText.Contains(searchFor, StringComparison.OrdinalIgnoreCase))
            {
                node.ForeColor = Program.ACTIVE_COLOR_SEARCH_FOREGROUND;
                node.BackColor = Program.ACTIVE_COLOR_SEARCH_HIGHLIGHT;
                if(expandToMakeVisible)
                {
                    node.EnsureVisible();
                }
            }
            if (SearchRecursive(node.Nodes, searchFor, expandToMakeVisible))
                return true;
        }
        return false;
    }

    private void ExpandAll(TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            node.EnsureVisible();
            if(node.Nodes.Count > 0)
            {
                ExpandAll(node.Nodes);
            }
        }
    }

    private void richTextBox1_TextChanged(object sender, EventArgs e)
    {

    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void button2_Click(object sender, EventArgs e)
    {
        ExpandAll(treeView1.Nodes);
    }

    private void button3_Click(object sender, EventArgs e)
    {
        if(datasource.ExportDataSnapshot is null)
        {
            richTextBox1.Text = $"There is no data available to export";
        }
        else
        {
            var here = Directory.GetCurrentDirectory();
            var filename = $"HnsExport_{DateTime.Now.ToFileTime()}.json";
            var filePath = Path.Join(here, filename);
            TextWriter txt = new StreamWriter(filePath);
            txt.Write(datasource.ExportDataSnapshot);
            txt.Close();
            richTextBox1.Text = $"Data saved to {filePath}";
        }
    }

    private void button4_Click(object sender, EventArgs e)
    {
        var path = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
        var pktmonScript = @"
            Write-Host -ForegroundColor DarkBlue '           __  __     _____         _                     '
            Write-Host -ForegroundColor DarkBlue '  /\  /\/\ \ \/ _\   / ___/ ___ __ | | ___  _ __ ___ _ __ '
            Write-Host -ForegroundColor Blue     ' / /_/ /  \/ /\ \   / __\ \/ / ''_ \| |/ _ \| ''__/ _ \ ''__|'
            Write-Host -ForegroundColor DarkCyan '/ __  / /\  / _\ \ / /___>  <| |_) | | (_) | | |  __/ |   '
            Write-Host -ForegroundColor Cyan     '\/ /_/\_\ \/  \__/ \ ___/_/\_\ .__/|_|\___/|_|  \___|_|   '
            Write-Host -ForegroundColor White    '                             |_|                          '

            Write-Host -ForegroundColor Cyan 'Using pktmon to create a packet capture'
            $pktmon = Get-Command 'pktmon' -ErrorAction 'SilentlyContinue'
            if($null -eq $pktmon) {
                Write-Host -ForegroundColor Red 'The executable pktmon is not available in the PATH on this system'
                Write-Host -ForegroundColor Red 'https://docs.microsoft.com/en-us/windows-server/networking/technologies/pktmon/pktmon'
                Write-Host -ForegroundColor Red -NoNewline ""`nPress ENTER to close this window ""
                Read-Host
                exit
            }
            $pktmonBusy = pktmon status | Where-Object { $_ -notlike '*not running*' }
            while ($pktmonBusy) {
                Write-Warning 'There is already a pktmon trace running, try again when this one is complete'
                Write-Host -ForegroundColor Yellow (Invoke-Expression 'pktmon status' | Out-String)
                Write-Host -ForegroundColor Yellow -NoNewline ""`nPress ENTER to try again ""
                Read-Host
                $pktmonBusy = pktmon status | Where-Object { $_ -notlike '*not running*' }
            }
            $interfaceList = pktmon list --json | ConvertFrom-Json | Foreach-Object {
                    $_ | Select-Object -ExpandProperty Components `
                       | Where-Object { $_.Type -like '*vNIC*' -or $_.Type -eq 'NetVsc' } `
                       | Select-Object -Property Id, Name
                } | Sort-Object -Property Id | Get-Unique -AsString
            Write-Host ""`n$(($interfaceList | Format-Table * | Out-String).Trim())`n""

            $foundAdapter = $false
            while(-not $foundAdapter) {
                Write-Host -ForegroundColor Cyan -NoNewline 'Enter the numberic Id of the adapter you want to trace from above: '
                $adapterId = Read-Host
                if($interfaceList.Id -contains $adapterId) {
                    $foundAdapter = $true
                } else {
                    Write-Warning ""'$adapterId' is not in the list of available adapter IDs [$($interfaceList.Id -join ', ')]""
                }
            }

            $filename = ""$(pwd)\pktmon_$((Get-Date).ToFileTime()).etl""
            $command = ""pktmon start --capture --comp $adapterId --pkt-size 0 -f $filename""
            Write-Host ""Running '$command'""
            Invoke-Expression $command

            Write-Host -ForegroundColor Cyan -NoNewline ""`nTrace is recording, press ENTER to stop ""
            Read-Host
            $command = 'pktmon stop'
            Write-Host ""Running '$command'""
            Invoke-Expression $command

            $command = ""pktmon etl2pcap $filename""
            Write-Host ""Running '$command'""
            Invoke-Expression $command

            Write-Host -ForegroundColor Cyan ""`nTrace data is available at:`n  '$($filename -replace '\.etl$', '.pcapng')'""
            Write-Host -ForegroundColor Cyan -NoNewline ""`nPress ENTER to close this window ""
            Read-Host
        ";
        ProcessStartInfo startInfo = new ProcessStartInfo(path);
        startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(pktmonScript);
        startInfo.UseShellExecute = true;
        Process.Start(startInfo);
    }

    private void button5_Click(object sender, EventArgs e)
    {
        var path = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
        var hcsTraceScript = @"
            Write-Host -ForegroundColor DarkRed '           __  __     _____         _                     '
            Write-Host -ForegroundColor DarkRed '  /\  /\/\ \ \/ _\   / ___/ ___ __ | | ___  _ __ ___ _ __ '
            Write-Host -ForegroundColor Red     ' / /_/ /  \/ /\ \   / __\ \/ / ''_ \| |/ _ \| ''__/ _ \ ''__|'
            Write-Host -ForegroundColor Red     '/ __  / /\  / _\ \ / /___>  <| |_) | | (_) | | |  __/ |   '
            Write-Host -ForegroundColor White   '\/ /_/\_\ \/  \__/ \ ___/_/\_\ .__/|_|\___/|_|  \___|_|   '
            Write-Host -ForegroundColor White   '                             |_|                          '

            Write-Host -ForegroundColor DarkRed 'Using Windows Performance Recorder to capture an ETL trace'
            $wpr = Get-Command 'wpr.exe' -ErrorAction 'SilentlyContinue'
            if($null -eq $wpr) {
                Write-Host -ForegroundColor Red 'The executable wpr.exe is not available in the PATH on this system'
                Write-Host -ForegroundColor Red 'https://docs.microsoft.com/en-us/windows-hardware/test/wpt/windows-performance-recorder'
                Write-Host -ForegroundColor Red -NoNewline ""`nPress ENTER to close this window ""
                Read-Host
                exit
            }
            $wprAvailable = wpr.exe -status | Where-Object { $_ -like '*is not recording*' }
            while (!$wprAvailable) {
                Write-Warning 'There is already a wpr.exe session running, try again when this one is complete'
                Write-Host -ForegroundColor Yellow (Invoke-Expression 'wpr.exe -status' | Out-String)
                Write-Host -ForegroundColor Yellow -NoNewline ""`nPress ENTER to try again ""
                Read-Host
                $wprAvailable = wpr.exe -status | Where-Object { $_ -like '*is not recording*' }
            }

            $traceConfig = ""<?xml version=`""1.0`"" encoding=`""utf-8`""?>
<WindowsPerformanceRecorder Version=`""1.0`"" Author=`""WPR Profile Generator`"" Copyright=`""Microsoft Corporation`"" Company=`""Microsoft Corporation`"">
    <Profiles>
        <SystemCollector Id=`""SystemCollector`"" Name=`""NT Kernel Logger`"">
            <BufferSize Value=`""1024`"" />
            <Buffers Value=`""48`"" />
        </SystemCollector>
        <EventCollector Id=`""EventCollector_HCSTraceSession`"" Name=`""HCSTraceSession`"">
            <BufferSize Value=`""512`"" />
            <Buffers Value=`""32`"" />
        </EventCollector>
        <SystemProvider Id=`""SystemProvider`"">
            <Keywords>
                <Keyword Value=`""Loader`"" />
                <Keyword Value=`""ProcessThread`"" />
            </Keywords>
        </SystemProvider>
        <EventProvider Id=`""EventProvider_HvSocketTraceProvider`"" Name=`""B2ED3BDB-CD74-5B2C-F660-85079CA074B3`"" NonPagedMemory=`""true`"">
            <Keywords>
                <Keyword Value=`""0x0`""/>
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""HCS_NoTransport_Provider`"" Name=`""80CE50DE-D264-4581-950D-ABADEEE0D340`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xF000FFFFF47D`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""HCS_Full_Provider`"" Name=`""80CE50DE-D264-4581-950D-ABADEEE0D340`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xF000FFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""HCS_VmComputeDll`"" Name=`""af7fd3a7-b248-460c-a9f5-fec39ef8468c`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xFFFFFFFFFFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""ContainerDll_Provider`"" Name=`""67EB0417-9297-42AE-A1D9-98BFEB359059`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xF000FFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""ContainerStorage_Provider`"" Name=`""2551390d-5927-5c84-6f0a-027a7e78d38d`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xF000FFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""HyperV_Worker`"" Name=`""51ddfa29-d5c8-4803-be4b-2ecb715570fe`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xFFFFFFFFFFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""HyperV_Worker_Telemetry`"" Name=`""06C601B3-6957-4F8C-A15F-74875B24429D`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xFFFFFFFFFFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""HyperV_Chipset`"" Name=`""de9ba731-7f33-4f44-98c9-6cac856b9f83`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xFFFFFFFFFFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""HyperV_GuestCrash`"" Name=`""9a02f67c-133d-58be-ecad-0d891d8b6fd1`"" Level=`""5`"">
            <Keywords>
                <Keyword Value=`""0xFFFFFFFFFFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""HyperV_GuestCrash_EventLog`"" Name=`""C7C9E4F7-C41D-5C68-F104-D72A920016C7`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xFFFFFFFFFFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""WerReporting`"" Name=`""cc79cf77-70d9-4082-9b52-23f3a3e92fe4`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xFFFFFFFFFFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <EventProvider Id=`""WerFaultReporting`"" Name=`""1377561d-9312-452c-ad13-c4a1c9c906e0`"" Level=`""6`"">
            <Keywords>
                <Keyword Value=`""0xFFFFFFFFFFFFFFFF`"" />
            </Keywords>
        </EventProvider>
        <Profile Id=`""HcsWer.Verbose.File`"" Name=`""HcsWer`"" Description=`""Windows error reporting`"" LoggingMode=`""File`"" DetailLevel=`""Verbose`"">
            <Collectors>
                <SystemCollectorId Value=`""SystemCollector`"">
                    <SystemProviderId Value=`""SystemProvider`"" />
                </SystemCollectorId>
                <EventCollectorId Value=`""EventCollector_HCSTraceSession`"">
                    <EventProviders>
                        <EventProviderId Value=`""WerReporting`"" />
                        <EventProviderId Value=`""WerFaultReporting`"" />
                    </EventProviders>
                </EventCollectorId>
            </Collectors>
        </Profile>
        <Profile Id=`""HcsWithHvSocket.Verbose.File`"" Name=`""HcsWithHvSocket`"" Description=`""HCS (host compute service) tracing with transport and hvsocket logging`"" LoggingMode=`""File`"" DetailLevel=`""Verbose`"">
            <Collectors>
                <SystemCollectorId Value=`""SystemCollector`"">
                    <SystemProviderId Value=`""SystemProvider`"" />
                </SystemCollectorId>
                <EventCollectorId Value=`""EventCollector_HCSTraceSession`"">
                    <EventProviders>
                        <EventProviderId Value=`""HCS_Full_Provider`"" />
                        <EventProviderId Value=`""HCS_VmComputeDll`"" />
                        <EventProviderId Value=`""HyperV_Worker`"" />
                        <EventProviderId Value=`""HyperV_Worker_Telemetry`"" />
                        <EventProviderId Value=`""HyperV_Chipset`"" />
                        <EventProviderId Value=`""HyperV_GuestCrash`"" />
                        <EventProviderId Value=`""HyperV_GuestCrash_EventLog`"" />
                        <EventProviderId Value=`""EventProvider_HvSocketTraceProvider`"" />
                    </EventProviders>
                </EventCollectorId>
            </Collectors>
        </Profile>
        <Profile Id=`""HcsArgon.Verbose.File`"" Name=`""HcsArgon`"" Description=`""HCS (host compute service) tracing for Windows Containers`"" LoggingMode=`""File`"" DetailLevel=`""Verbose`"">
            <Collectors>
                <SystemCollectorId Value=`""SystemCollector`"">
                    <SystemProviderId Value=`""SystemProvider`"" />
                </SystemCollectorId>
                <EventCollectorId Value=`""EventCollector_HCSTraceSession`"">
                    <EventProviders>
                        <EventProviderId Value=`""HCS_NoTransport_Provider`"" />
                        <EventProviderId Value=`""ContainerDll_Provider`"" />
                        <EventProviderId Value=`""ContainerStorage_Provider`"" />
                        <EventProviderId Value=`""WerReporting`"" />
                    </EventProviders>
                </EventCollectorId>
            </Collectors>
        </Profile>
    </Profiles>
</WindowsPerformanceRecorder>
""
            $profilePath = ""$(pwd)\wpr_profile.wprp""
            Set-Content -Force -Path $profilePath -Value $traceConfig

            $filename = ""$(pwd)\wpr_$((Get-Date).ToFileTime()).etl""
            $command = ""wpr.exe -start $profilePath!HcsArgon -start $profilePath!HcsWithHvSocket -start $profilePath!HcsWer -filemode""
            Write-Host ""Trace command is:`n'$command'""
            Write-Host -NoNewline ""Press ENTER to start the trace ""
            Read-Host
            Invoke-Expression $command

            Write-Host -ForegroundColor DarkRed -NoNewline ""`nTrace is recording, press ENTER to stop ""
            Read-Host
            $command = ""wpr.exe -stop `""$filename`"" `""Automated HCS trace from HNS explorer`""""
            Write-Host ""Running '$command'""
            Invoke-Expression $command

            if((Get-Command 'wpa.exe' -ErrorAction 'SilentlyContinue')) {
                $command = ""wpa.exe `""$filename`""""
                Write-Host ""Running '$command'""
                Invoke-Expression $command
            } else {
                Write-Host 'Windows Performance Analyzer is not installed, you need to install it to view the trace file'
                Write-Host 'https://docs.microsoft.com/en-us/windows-hardware/test/wpt/windows-performance-analyzer'
            }

            Write-Host -ForegroundColor DarkRed ""`nTrace is saved at '$filename' to be opened by Windows Performance Analyzer""
            Write-Host -ForegroundColor DarkRed -NoNewline ""`nPress ENTER to close this window ""
            Read-Host
        ";
            
        ProcessStartInfo startInfo = new ProcessStartInfo(path);
        startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(hcsTraceScript);
        startInfo.UseShellExecute = true;
        Process.Start(startInfo);
    }
}
