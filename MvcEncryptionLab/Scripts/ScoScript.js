
var testKey = "8YMiP/3jSj6Zfe79lM8x0GqKOmbo9gR5qurmh68FqmY=";

function promptForKey(keyExists) {

    if (!keyExists) {
        BootstrapDialog.show({
            closable: false,
            title: 'ACA-IRS File Processer',
            message: 'This is the first time a security key has been entered. Please enter the value twice below.'
                + '<fieldset class="form-group">'
                + '   <label for="key1">Security Key</label>'
                + '   <textarea class="form-control" id="key1" rows="1" style="min-width: 100%">' + testKey + '</textarea>'
                + '   <label for="key2">Re-enter Security Key</label>'
                + '   <textarea class="form-control" id="key2" rows="1" style="min-width: 100%">' + testKey + '</textarea>'
                + '</fieldset>',
            buttons: [
                {
                    label: 'OK',
                    action: function (dialogRef) {
                        var key1 = dialogRef.getModalBody().find('#key1').val();
                        var key2 = dialogRef.getModalBody().find('#key2').val();

                        if (key1 === key2) {
                            if (!validateKey(key1)) {
                                return false;
                            }
                            $.ajax({
                                url: "/Default/PostSecurityKey",
                                type: "POST",
                                data: { 'key': key1 },
                                dataType: "json",
                                success: function (data) {
                                    alert("Hash of key has been saved to database and will be used to validate future entires of the key.");
                                },
                                error: function (xhr, httpStatusMessage, customErrorMessage) {
                                    alert("Error processing security key: " + customErrorMessage);
                                    gotoHomePage();
                                }
                            });
                            
                        } else {
                            alert("Key values do not match.");
                        }
                    }
                },
                {
                    // Cancel entry of key
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
            message: 'Please enter security key: <textarea class="form-control" id="key" rows="1" style="min-width: 100%" ></textarea>',
            buttons: [
                {
                    label: 'OK',
                    action: function (dialogRef) {
                        var key = dialogRef.getModalBody().find('#key').val();

                        if (!validateKey(key)) {
                            return false;
                        }

                        $.ajax({
                            url: "/Default/PostSecurityKey",
                            type: "POST",
                            data: { 'key': key },
                            dataType: "json",
                            success: function (data) {
                            },
                            error: function (xhr, httpStatusMessage, customErrorMessage) {
                                alert("Error processing security key: " + customErrorMessage);
                                gotoHomePage();
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
    return true;
}