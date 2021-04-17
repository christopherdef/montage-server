# Montage: Server



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
TODO

## **Contributors**
Nick Howell, Chris de Freitas,
Gavin Monson,
Andrew Fryzel