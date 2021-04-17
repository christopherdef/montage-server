# Montage

## **Contributors**
Nick Howell, Chris de Freitas,
Gavin Monson,
Andrew Fryzel



## **Summary**  
Creating an algorithm that reduces the need for editors to re-watch dozens of hours of footage can cut back significantly on the time an editor spends on editing, and in turn, improve project productivity. Additionally, by providing a clean, intuitive, and simple way of cutting and re-cutting footage together, Montage would provide editor’s an entirely new way to edit which would allow them to focus solely on the story they want to tell, rather than the tedious intricacies of editing their footage manually.

## **Abstract**  
Video editors often work with huge libraries of footage to create memorable and entertaining experiences. Any movie theatre blockbuster with a 90 minute runtime started out as several thousands of hours of chaotic, disorganized footage spread over a dozen hard drives. Cutting and labeling a video or movie from those clips is a monumental task. For editors, the biggest time sink comes from two problems: editors don’t know what’s in a clip until they watch it, and editors have limited human memory. Editors are asked to fix these problems by wasting their hours away watching and rewatching footage. These massive video reviews take away valuable time editors could otherwise be using to edit the footage they have. Given the complex, tedious, and often inefficient nature of video review and editing, we present a simple and elegant solution: Montage.  

Montage is an Adobe Premiere extension created to streamline the editing process in two important  ways. First, Montage incorporates the best of modern speech-to-text and information retrieval technology to instantly find footage corresponding to sentences, individuals, or topics and efficiently captions and tags each second of footage. Montage provides all the tools needed to know the footage inside and out and, in turn, makes for better editing and happier editors. However, all this rich and concise information would go to waste without Montage’s second unique feature: the integrated timeline video editor. The Montage extension will allow users to rearrange, cut, and delete parts of the transcript and have those changes reflect in the premier timeline, allowing an editor to edit footage without even touching the video. On top of that, Montage’s quick clip preview would enable an editor to drag and drop various clips of footage directly onto their timeline. Users would also be able to search through thousands of hours of footage for specific words or objects using our advanced Machine Learning and Natural Language Processing algorithms. Montage’s accessible interface and powerful features make it a valuable resource for editors working on any project.

# Server

## **Installation**

The Montage server needs the addition of user secrets. This can be accomplished by adding secrets to the secrets.json file which can be found at:

"C:\Users\\"username"\AppData\Roaming\Microsoft\UserSecrets\aspnet-MontageServer-7F9ACA9F-09C7-416D-B4A1-DC70A4BB5889"

To add secrets manually, navigate to the root directory of the project and run:

"dotnet user-secrets set SendGridUser montageextension@gmail.com

dotnet user-secrets set SendGridKey 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

dotnet user-secrets set "Authentication:Google:ClientId" 1052455296366-c94ad5uks877ie7svdf0fftr2hpvrfpr.apps.googleusercontent.com

dotnet user-secrets set "Authentication:Google:ClientSecret" Locr5siDpTB75EYNhkHkDnUF"

## **Usage**

Run the server. This can be done, for example,  by running IIS Express in Visual Studios. The server will run by default on localhost:44330.

# Extension

Coding can be done with any text editor, we are going to use Visual Studio Code, to code with but really any text editor will work.
Step 1
Clone the remote repository on gitlab (repository located below, also included are the clone commands to be run with git): 
https://capstone-cs.eng.utah.edu/montage/montage-project

SSH Clone:
git@capstone-cs.eng.utah.edu:montage/montage-project.git

OR

HTTPS Clone:
https://capstone-cs.eng.utah.edu/montage/montage-project.git

As a note these can be cloned anywhere on  your computer that you want, except for the path in step 3, as an adobe extension does not like .git file types and as a result will not recognize it as an extension.
Step 2
On windows go to search either using the windows button or clicking in the left hand corner, and search for “Registry Editor” once there go to:

(DEPENDING ON VERSION ADOBE PREMIERE YOU MAY HAVE TO CHANGE THE CSXS.10 FILE NOT THE CSXS.9) These would be the exact same changes below just switching CSXS.9 with CSXS.10, if you go to this folder and see both only change the higher number, if you see one or the other change the one you see. Just know if you update your Adobe Premiere, and extensions are not working this would be the first place to check, to make sure CSXS.10 is correct if before you only had CSXS.9.
HKEY_CURRENT_USER > SOFTWARE > Adobe > CSXS.9 


After you have located this folder you will right click on CSXS.9 and select > new > string value


Rename it to “PlayerDebugMode” with a value of 1

Step 3
Then go to your downloaded repository and copy the folder called PProPanel (this will be in the repository one level in to wherever the repository was cloned to)

Just copying PProPanel we will then go to:
C:\Program Files (x86)\Common Files\Adobe\CEP\extensions\PProPanel
This is the default path, so if it is something you changed the default location for, you are looking for the CEP\extensions in the Adobe program files.

Step 4
Once copied we are now able to go into an Adobe Premiere project, and select windows > extensions and then the two montage extensions to run and view your changes made to the code. 


Just make sure you are copying over that directory everytime you make changes otherwise you will not see your changes reflected. Below is what the extensions will look like these can be dragged into any section inside of adobe premiere so they are not individual windows etc., but you are now able to see and run Montage’s extension.



