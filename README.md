# CB-API-TinyPointOfSale-Mobile-Devices-App
This project contains sample code (C#) for an application demonstrating how to use the APIs needed for a Point of Sale application that integrates with Cloudbeds. The application is targeted at Android and iOS platforms and written in C#. The application uses the Xamarin mobile app framework which can be installed on top of Visual Studio.

1. To use this application you will need to FIRST use the **CB-API-Explorer** (https://github.com/cloudbeds/CB-API-Explorer) sample to create 2 XML files: (1) An Application-Secrets file containing the Application ID and Secret.  (2) A Session-Tokens file that contains the access tokens (i.e. secrets) this application uses to log into your Cloudbeds property:

    i. Load the CB-API-Explorer sample and run it
    
    ii. Click on the "Bootstrap to create user access tokens" button and follow the instructions
    
    iii. Click on the "Save the user access tokens to storage" button to store the access tokens in a local file
    
    iv. You will need to Copy/Paste the contents if these XML files into the Mobile applications. If you are running the Android emulator you should be able to copy/paste right from your desktop into the Textbox of the mobile application and click the mobile-app start-page buttons to parse and save these secrets-containing XML files onto the mobile device.  (If you are running on actual hardware devices, you can email yourself these 2 files and copy/paste them from the mobile device itself). You must do this for both the Application-Secrets and the Session-Tokens; there is a button on the mobile application home screen to help import each. 
    
    **Result:** You will now be able to run THIS (CB-API-TinyPointOfSale-Mobile-Devices-App) application and it will use these secrets to log into your Cloudbeds property
   


2. This application implements a VERY SIMPLE Point of Sale system:

     i. You can choose menu items to add to a guest's bill
     
     ii. You can query Cloudbeds for the list of current Guests registered in your property, and select a guest to assign the bill to
     
     iii. The guest can add an optional gratuity to the bill
     
     iv. You can submit this bill to Cloudbeds to add to the guest's folio as a charge.
     
         - All the menu items added to the guest’s bill are stored as line items in the guest's folio
         
         - The notes field in each line item is written to show that the items belong grouped together
         
         - A unique ID is stored with the folio entry to prevent duplicate submissions of the same bill
         
       
