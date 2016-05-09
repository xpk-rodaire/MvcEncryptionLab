
var testKey = "8YMiP/3jSj6Zfe79lM8x0GqKOmbo9gR5qurmh68FqmY=";
var testPhrase = "Our greatest weakness lies in giving up. The most certain way to succeed is always to try just one more time. Thomas A. Edison";

function promptForKey(promptForCheckPhrase) {

    if (promptForCheckPhrase) {
        BootstrapDialog.show({
            closable: false,
            title: 'ACA-IRS File Processer',
            message: 'Please enter security key and phrase that will be used to validate future entires of the security key (minimum length 100 characters):'
                + '<fieldset class="form-group">'
                + '   <label for="securityKey">Security Key</label>'
                + '   <textarea class="form-control" id="securityKey" rows="1" style="min-width: 100%">' + testKey + '</textarea>'
                + '   <label for="phrase">Phrase</label>'
                + '   <textarea class="form-control" id="phrase" rows="3" style="min-width: 100%">' + testPhrase + '</textarea>'
                + '</fieldset>',

            buttons: [
                {
                    label: 'OK',
                    action: function (dialogRef) {
                        var key = dialogRef.getModalBody().find('textarea').val();
                        var phrase = dialogRef.getModalBody().find('textarea').val();

                        if (!validateKey(key)) {
                            return false;
                        }

                        if (!validatePhrase(phrase)) {
                            return false;
                        }

                        dialogRef.close();

                        $.ajax({
                            url: "/Default/PostSecurityItems/",
                            type: "POST",
                            data: { 'key': key, 'value': phrase },
                            dataType: "json",
                            success: function (data) {
                                alert("Hello");
                            },
                            error: function (xhr, httpStatusMessage, customErrorMessage) {
                                alert("Status = " + xhr.status + ", Message = " + customErrorMessage);
                            }
                        });
                    }
                },
                {
                    // Cancel entry of key and phrase
                    label: 'Cancel',
                    action: function (dialogRef) {
                        dialogRef.close();
                        gotoHomePage();
                    }
                }
            ]
        });

    } else {

        BootstrapDialog.show({
            closable: false,
            size: BootstrapDialog.SIZE_LARGE,
            title: 'ACA-IRS File Processer',
            message: 'Please enter security key: <textarea class="form-control" rows="1" style="min-width: 100%" ></textarea>',
            buttons: [
                {
                    label: 'OK',
                    action: function (dialogRef) {
                        var key = dialogRef.getModalBody().find('textarea').val();

                        if (!validateKey(key)) {
                            return false;
                        }

                        $.ajax({
                            url: "/Default/PostEncryptionKey",
                            type: "POST",
                            data: { 'key': key },
                            dataType: "json",
                            success: function (data) {
                                if (data.status == "ValidateCheckPhrase") {
                                    BootstrapDialog.show({
                                        closable: false,
                                        title: 'ACA-IRS File Processer',
                                        message: 'Does this match the phrase originally entered?'
                                            + '<fieldset class="form-group">'
                                            + '   <label for="securityKey">Security Key Entered</label>'
                                            + '   <textarea class="form-control" id="securityKey" rows="1" style="min-width: 100%" readonly>' + data.key + '</textarea>'
                                            + '   <label for="phrase">Decrypted Phrase</label>'
                                            + '   <textarea class="form-control" id="phrase" rows="3" style="min-width: 100%" readonly>' + data.phrase + '</textarea>'
                                            + '</fieldset>',
                                        buttons: [
                                            {
                                                label: 'Yes',
                                                action: function (dialogRef) {
                                                    dialogRef.close();
                                                    $.ajax({
                                                        url: "/Default/PostCheckPhraseResponse/",
                                                        type: "POST",
                                                        data: { 'key': data.key },
                                                        dataType: "json",
                                                        success: function (data) {

                                                        }
                                                    });
                                                }
                                            },
                                            {
                                                // User indicates phrase does not match
                                                label: 'No',
                                                action: function (dialogRef) {
                                                    alert("Please review the security key entered and try again.");
                                                    dialogRef.close();
                                                    gotoHomePage();
                                                }
                                            }
                                        ]
                                    });
                                }
                                else {
                                    alert("Invalid response from server - please contact ISD support.");
                                    gotoHomePage();
                                }
                            },
                            error: function (xhr, httpStatusMessage, customErrorMessage) {
                                alert("Status = " + xhr.status + ", Message = " + customErrorMessage);
                            }
                        });
                        dialogRef.close();
                    }
                },
                {
                    // Cancel entry of security key
                    label: 'Cancel',
                    action: function (dialogRef) {
                        dialogRef.close();
                        gotoHomePage();
                    }
                }
            ]
        });
    }
}

function gotoHomePage() {
    window.location.replace("/Default/Index/");
}

function validateKey(value)
{
    if ($.trim(value.toLowerCase()) === '') {
        alert('Please enter a key or press Cancel.');
        return false;
    }
    else if (/\s/.test(value)) {
        alert('Key cannot contain any white space (spaces, tabs, line returns, etc.)');
        return false;
    }
}

function validatePhrase(value)
{
    if ($.trim(value.toLowerCase()) === '') {
        alert('Please enter a phrase or press Cancel.');
        return false;
    }
    else if (value.length < 100) {
        alert('Phrase must be at least 100 characters in length.');
        return false;
    }
}