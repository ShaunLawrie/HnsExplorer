using HnsExplorer.Extensions;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HnsExplorer;

public partial class SummaryForm : Form
{
    private static string? EXPORT_DATA_SNAPSHOT;

    public SummaryForm()
    {
        InitializeComponent();
        LoadTreeview();
        textBox1.KeyPress += new KeyPressEventHandler(CheckEnterKeyPress);
        textBox1.Focus();
        textBox1.Select();
    }

    private void LoadTreeview()
    {
        

        var activitiesData = Program.HnsAccess.GetActivities();
        var namespaceData = Program.HnsAccess.GetNamespaces();
        var networkData = Program.HnsAccess.GetNetworks();
        var policyData = Program.HnsAccess.GetPolicyLists();
        var endpointData = Program.HnsAccess.GetEndpoints();
        var computeData = Program.HcsAccess.GetComputeSystems();

        var summaryOutput = $"Activities: {activitiesData.Count()}{Environment.NewLine}";
        summaryOutput += $"Namespaces: {namespaceData.Count()}{Environment.NewLine}";
        summaryOutput += $"Networks: {networkData.Count()}{Environment.NewLine}";
        summaryOutput += $"Network policies: {policyData.Count()}{Environment.NewLine}";
        summaryOutput += $"Network endpoints: {endpointData.Count()}{Environment.NewLine}";
        summaryOutput += $"Compute systems: {computeData.Count()}{Environment.NewLine}";

        richTextBox1.Text = summaryOutput;

        treeView1.Nodes.Clear();

        var activitiesNode = new TreeNode
        {
            Text = "Activities",
            Tag = summaryOutput
        };
        var orphanedActivities = activitiesNode.Nodes.InsertNestedChildren(activitiesData, "ID", "parentId", "Activity", "Allocators.Tag");
        var orphanedNetworks = activitiesNode.Nodes.InsertNestedChildren(networkData, "ID", "Resources.ID", "Network", "Name");
        var orphanedEndpoints = activitiesNode.Nodes.InsertNestedChildren(endpointData, "ID", "Resources.ID", "Endpoint", "Name");
        var orphanedPolicies = activitiesNode.Nodes.InsertNestedChildren(policyData, "ID", "Resources.ID", "Policy", "Name");
        treeView1.Nodes.Add(activitiesNode);

        var orphansNode = new TreeNode("Orphaned Data");
        orphansNode.Nodes.InsertChildren(orphanedActivities, "ID", "Activities", "Allocators.Tag");
        orphansNode.Nodes.InsertChildren(orphanedNetworks, "ID", "Network", "Name");
        orphansNode.Nodes.InsertChildren(orphanedEndpoints, "ID", "Endpoint", "Name");
        orphansNode.Nodes.InsertChildren(orphanedPolicies, "ID", "Endpoint", "Name");

        var endpointIds = new List<string>();
        foreach (var item in endpointData)
        {
            var id = item.GetJsonDataAsString("ID");
            endpointIds.Add(id);
        }
        var endpointStatsData = Program.HnsAccess.GetEndpointStats(endpointIds);
        activitiesNode.Nodes.InsertNestedChildren(endpointStatsData, "InstanceId", "EndpointId", "Endpoint Stats", "Name");

        var orphanedCompute = treeView1.Nodes.InsertChildrenWithMatchingParentReference(computeData, endpointData, "Id", "ID", "VirtualMachine", "Virtual Machine", "Owner");
        orphanedCompute = treeView1.Nodes.InsertChildrenWithMatchingParentReference(orphanedCompute, endpointData, "Id", "ID", "SharedContainers", "Container", "Owner");
        orphansNode.Nodes.InsertChildren(orphanedCompute, "ID", "Compute", "Name");
        var namespaceNode = new TreeNode
        {
            Text = "Namespaces",
            Tag = $"{namespaceData.Count()} namespaces"
        };
        namespaceNode.Nodes.InsertChildren(namespaceData, "ID", "Namespace", "CompartmentId");
        treeView1.Nodes.Add(namespaceNode);

        if (orphansNode.Nodes.Count > 0)
        {
            treeView1.Nodes.Add(orphansNode);
        }

        var allData = new Dictionary<string, IEnumerable<JsonElement>>();
        allData.Add("Namespaces", namespaceData);
        allData.Add("Network", networkData);
        allData.Add("Policies", policyData);
        allData.Add("Endpoints", endpointData);
        allData.Add("Compute", computeData);
        allData.Add("Activities", activitiesData);
        allData.Add("EndpointStats", endpointStatsData);
        EXPORT_DATA_SNAPSHOT = JsonSerializer.Serialize(allData);
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
            SearchRecursive(treeView1.Nodes, textBox1.Text);
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

    private bool SearchRecursive(TreeNodeCollection nodes, string searchFor)
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
                node.EnsureVisible();
            }
            if (SearchRecursive(node.Nodes, searchFor))
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
        if(EXPORT_DATA_SNAPSHOT is null)
        {
            richTextBox1.Text = $"There is no data available to export";
        }
        else
        {
            var here = Directory.GetCurrentDirectory();
            var filename = $"HnsExport_{DateTime.Now.ToFileTime()}.json";
            var filePath = Path.Join(here, filename);
            TextWriter txt = new StreamWriter(filePath);
            txt.Write(EXPORT_DATA_SNAPSHOT);
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

            Write-Host -ForegroundColor Cyan -NoNewline ""`nTrace is recording, press ENTER to stop""
            Read-Host
            $command = 'pktmon stop'
            Write-Host ""Running '$command'""
            Invoke-Expression $command

            $command = ""pktmon etl2pcap $filename""
            Write-Host ""Running '$command'""
            Invoke-Expression $command

            Write-Host -ForegroundColor Cyan ""`nTrace data is available at:`n  '$($filename -replace '\.etl$', '.pcapng')'""
            Write-Host -ForegroundColor Cyan -NoNewline ""`nPress ENTER to close this window""
            Read-Host
        ";
        ProcessStartInfo startInfo = new ProcessStartInfo(path);
        startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        startInfo.ArgumentList.Add("-C");
        startInfo.ArgumentList.Add(pktmonScript);
        startInfo.UseShellExecute = true;
        Process.Start(startInfo);
    }
}
