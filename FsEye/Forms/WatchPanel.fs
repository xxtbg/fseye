﻿(*
Copyright 2011 Stephen Swensen

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*)
namespace Swensen.FsEye.Forms
open System.Windows.Forms
open System.Reflection
open Swensen.FsEye

type WatchPanel() as this =
    inherit Panel()    
    let continueButton = new Button(Text="Async Continue", AutoSize=true, Enabled=false)
    let asyncBreak = async {
        let! _ = Async.AwaitEvent continueButton.Click
        ()
    }

    let splitContainer = new SplitContainer(Dock=DockStyle.Fill, Orientation=Orientation.Vertical)

    let hidePanel2 () =
        splitContainer.Panel2Collapsed <- true
        splitContainer.Panel2.Hide()

    let showPanel2 () =
        splitContainer.Panel2Collapsed <- false
        splitContainer.Panel2.Show()

    //Auto-update splitter distance to a percentage on resize
    let mutable splitterDistancePercentage = 0.5
    do     
        let updateSplitterDistancePercentage() = splitterDistancePercentage <- (float splitContainer.SplitterDistance) / (float splitContainer.Width)
        let updateSplitterDistance() = splitContainer.SplitterDistance <- int ((float splitContainer.Width) * splitterDistancePercentage)
        updateSplitterDistance() //since SplitterMoved fires first, need to establish default splitter distance
        splitContainer.SplitterMoved.Add(fun _ -> updateSplitterDistancePercentage())
        this.SizeChanged.Add(fun _ -> updateSplitterDistance())

        hidePanel2()

    let tabControl = new TabControl(Dock=DockStyle.Fill)
    let pluginManager = new PluginManager()
    //wire up tab closing, coordinating with the plugin manager.
    do
        let closeTab (tab:TabPage) = 
            pluginManager.RemoveManagedWatchViewer(tab.Name)

        let closeOtherTabs (tab:TabPage) =
            tabControl.TabPages
            |> Seq.cast<TabPage>
            |> Seq.filter (fun x -> x.Name <> tab.Name)
            |> Seq.map (fun x -> x.Name)
            |> Seq.toList
            |> Seq.iter (fun id -> pluginManager.RemoveManagedWatchViewer(id))

        let closeAllTabs () =
            tabControl.TabPages
            |> Seq.cast<TabPage>
            |> Seq.map (fun x -> x.Name)
            |> Seq.toList
            |> Seq.iter (fun id -> pluginManager.RemoveManagedWatchViewer(id))

        //we may want to have WatchUpdating event and trigger select at that point rather than after
        pluginManager.WatchUpdated.Add (fun mwv -> 
            tabControl.SelectTab(mwv.ID)
        )

        pluginManager.WatchAdded.Add (fun mwv -> 
            //display the watch viewer
            let tabPage = new TabPage(mwv.ID, Name=mwv.ID)
            let wvControl = mwv.WatchViewer.Control
            wvControl.Dock <- DockStyle.Fill
            tabPage.Controls.Add(wvControl)
            tabControl.TabPages.Add(tabPage)
            tabControl.SelectTab(tabPage)

            if tabControl.TabCount > 0 && splitContainer.Panel2Collapsed then
                showPanel2()
        )
        
        pluginManager.WatchRemoved.Add (fun mwv ->
            let tab = tabControl.TabPages.[mwv.ID]
            tab.Dispose() //http://stackoverflow.com/a/1970158/236255
            tabControl.TabPages.Remove(tab)
            
            if tabControl.TabCount = 0 && not splitContainer.Panel2Collapsed then
                hidePanel2()
        )
        
        let createTabContextMenu (tab:TabPage) =
            new ContextMenu [|
                let mi = new MenuItem("Close Tab") 
                mi.Click.Add(fun _ -> closeTab tab) 
                yield mi

                let mi = new MenuItem("Close Other Tabs", Enabled=(tabControl.TabCount>1))
                mi.Click.Add(fun _ -> closeOtherTabs tab)
                yield mi

                let mi = new MenuItem("Close All Tabs") 
                mi.Click.Add(fun _ -> closeAllTabs ()) 
                yield mi
            |]
        
        tabControl.MouseClick.Add (fun e -> 
            if e.Button = MouseButtons.Right then                                                 
                let clickedTab = 
                    tabControl.TabPages 
                    |> Seq.cast<TabPage> 
                    |> Seq.mapi (fun i tab -> (i,tab)) 
                    |> Seq.find (fun (i,tab) -> tabControl.GetTabRect(i).Contains(e.Location))
                    |> snd
                (createTabContextMenu clickedTab).Show(tabControl, e.Location))

    
    let treeView = new WatchTreeView(Some(pluginManager), Dock=DockStyle.Fill)

    do

        splitContainer.Panel1.Controls.Add(treeView)
        splitContainer.Panel2.Controls.Add(tabControl)

        //must add splitContainer (with dockstyle fill) first in order for it to be flush with button panel
        //see: http://www.pcreview.co.uk/forums/setting-control-dock-fill-you-have-menustrip-t3240577.html
        this.Controls.Add(splitContainer)
        do
            let buttonPanel = new FlowLayoutPanel(Dock=DockStyle.Top, AutoSize=true)
            do
                let archiveButton = new Button(Text="Archive Watches", AutoSize=true)
                archiveButton.Click.Add(fun _ -> this.Archive()) 
                buttonPanel.Controls.Add(archiveButton)
            do
                let clearButton = new Button(Text="Clear Archives", AutoSize=true)
                clearButton.Click.Add(fun _ -> this.ClearArchives() ) 
                buttonPanel.Controls.Add(clearButton)
            do
                let clearButton = new Button(Text="Clear Watches", AutoSize=true)
                clearButton.Click.Add(fun _ -> this.ClearWatches()) 
                buttonPanel.Controls.Add(clearButton)
            do
                let clearButton = new Button(Text="Clear All", AutoSize=true)
                clearButton.Click.Add(fun _ -> this.ClearAll()) 
                buttonPanel.Controls.Add(clearButton)
            do
                continueButton.Click.Add(fun _ -> continueButton.Enabled <- false)
                buttonPanel.Controls.Add(continueButton)
            this.Controls.Add(buttonPanel)
    with
        //a lot of delegation to treeView below -- not sure how to do this better

        ///Add or update a watch with the given name, value, and type.
        member this.Watch(name, value, ty) =
            treeView.Watch(name, value, ty)

        ///Add or update a watch with the given name and value.
        member this.Watch(name, value) =
            treeView.Watch(name,value)

        ///Take archival snap shot of all current watches using the given label.
        member this.Archive(label: string) =
            treeView.Archive(label)

        ///Take archival snap shot of all current watches using a default label based on an archive count.
        member this.Archive() = 
            treeView.Archive()

        ///Clear all archives and reset the archive count.        
        member this.ClearArchives() = 
            treeView.ClearArchives()

        ///Clear all watches (doesn't include archive nodes).
        member this.ClearWatches() = 
            treeView.ClearWatches()

        ///Clear all archives (reseting archive count) and watches.
        member this.ClearAll() = 
            treeView.ClearAll()

        //note: would like to use [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
        //on the following two Async methods but the attribute is not valid on methods
        //maybe we should introduce our own "MethodDebuggerAttribute"

        ///<summary>
        ///Use this in a sync block with do!, e.g.
        ///<para></para>
        ///<para>async { </para>
        ///<para>&#160;&#160;&#160;&#160;for i in 1..100 do</para>
        ///<para>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;watch.Watch("i", i, typeof&lt;int&gt;)</para>
        ///<para>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;watch.Archive()</para>
        ///<para>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;if i = 50 then</para>
        ///<para>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;do! watch.AsyncBreak()</para>
        ///<para>} |> Async.StartImmediate</para>
        ///</summary>
        member this.AsyncBreak() =
            continueButton.Enabled <- true
            asyncBreak

        ///Continue from an AsyncBreak()
        member this.AsyncContinue() =
            //the Click event for continueButton.PerformClick() doesn't fire when form is closed
            //but it does fire using InvokeOnClick
            this.InvokeOnClick(continueButton, System.EventArgs.Empty)