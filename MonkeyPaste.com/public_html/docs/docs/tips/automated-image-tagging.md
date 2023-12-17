# Automated Image Tagging
Do you want a way to automatically classify the things you copy into organized groups so you can use them later?
Well this tip is for you then!

<p align="center">
  <video controls height="300">
    <source src="/docs/build/videos/vs_code_custom_write_from_paste_bar.mp4"/>
  </video>
</p>

This tip will show you how to create triggers, actions and use the [Image Annotator](https://www.github.com/monkeypaste/ImageAnnotator) available for download from the *Plugin Browser* to add a 'Monkey' tag to any image we copy with a monkey in it!

:::info
The [Image Annotator](https://www.github.com/monkeypaste/ImageAnnotator) plugin detects common objects in images and provides a name, box and score (between 0 and 1 of how sure it is about the name) for each object it detects. 
:::

Here's how:
## Adding the Plugin
1. Open the *Analyzer* sidebar and click the 🧩 button to reveal the *Plugin Browser*
2. Type 'Image Annotator' into the search box then select the *Browse* tab
3. Select 'Image Annotator' in the left pane and click the *Install* button from the right pane.
4. After a moment the plugin will be installed and ready for use.
## Creating the Monkeys tag
1. Open the *Collections* sidebar and select the top-level *Tags* collection.
2. You'll now see a ➕ button in the top-left of the sidebar. Click the ➕ button to add a new tag.
3. A new tag named 'Untitled' will be added to the bottom of the list, right-click it and select *Rename* and change its name to 'Monkeys'
## Setup the Clip Added Trigger
1. Now open the *Triggers* sidebar click the ➕ button on the top-right of the sidebar to show the *Create Trigger* menu
2. Select the *Clip Added* trigger to create the new trigger
3. If its not automatically selected, select the 'Clip Added Trigger' from the *Trigger Selector* below the ➕ button
4. Scroll down to show the *Action Properties* view and click the 'Clip Added Trigger' label and rename it 'Monkey Tag Trigger'.
5. Then below in the properties, select 'Image' for the *Trigger* parameter since for this example we want to tag pictures of monkeys.
## Add the ImageAnnotator Action
1. We need a way to run the 'Image Annotator' plugin we just installed. To do this we add an *Analyze* action to the 'Monkey Tag Trigger' by right-clicking the green circle on right in the *Action Designer* view and selecting *Add->Analyze* from the *Add Action* pop-menu.
2. The new 'Analyze1' action will become selected and shown with an arrow pointing to it from the 'Monkey Tag Trigger'
3. Let's rename it to 'Detect Image Objects' back in *Action Properties*
4. Now to use the Image Annotator, click the *Component Selector* for the *Analyzer* parameter and select *Image Annotator->Default Annotator*.

:::tip
Analyzer settings are stored using presets so you can re-use certain configurations later. All plugins will have at least 1 default preset (like we just references from the *Component Selector*). You can add new presets at anytime by clicking the *Add Preset* button at the bottom of the *Analyzer* sidebar. 
:::

## Using a Conditional Action
To recap, we have a trigger setup so when an *Image* is added it will be analyzed the *Image Annotator* which (among other things) outputs a list of the objects it detected. We only care about 'monkey' objects so now we'll add that logic as a new *Conditional* action.
1. Right-click the 'Detect Image Objects' square in the *Action Designer* and select the *Add->Conditional* option so a new  



<p align="center">
  <img src="/docs/build/img/ole_format_button_write_menu_text_button.png" width="300"/>  
</p>  

