
function promptForKey() {
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
                    if ($.trim(key.toLowerCase()) === '') {
                        alert('Please enter a value or press Cancel.');
                        return false;
                    }
                    else if (/\s/.test(key)) {
                        alert('Value cannot contain any white space (spaces, tabs, line returns, etc.)');
                        return false;
                    }

                    $.ajax({
                        url: "/Default/PostEncryptionKey",
                        type: "POST",
                        data: { 'key': key },
                        dataType: "json",
                        success: function (data) {
                            if (data.status == "ValidatePassPhrase") {
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
                                                //$.ajax({
                                                //    url: "/Default/PostCheckPhraseResponse/",
                                                //    type: "POST",
                                                //    data: { 'value': true },
                                                //    dataType: "json",
                                                //    success: function (data) {

                                                //    }
                                                //});
                                            }
                                        },
                                        {
                                            // User indicates phrase does not match - expire user's security key
                                            label: 'No',
                                            action: function (dialogRef) {
                                                alert("Please review the security key entered and try again.");
                                                dialogRef.close();
                                                $.ajax({
                                                    url: "/Default/PostCheckPhraseResponse/",
                                                    type: "POST",
                                                    data: { 'value': false },
                                                    dataType: "json",
                                                    success: function (data) {
                                                    }
                                                });
                                            }
                                        }
                                    ]
                                });
                            }
                            else if (data.status == "PromptForPassPhrase")
                            {
                                BootstrapDialog.show({
                                    closable: false,
                                    title: 'ACA-IRS File Processer',
                                    message: 'Please enter a phrase that will be used to validate future entires of the security key (minimum size 100 characters):<input type="text" class="form-control"/>',
                                    buttons: [
                                        {
                                            label: 'OK',
                                            action: function (dialogRef) {
                                                var phrase = dialogRef.getModalBody().find('input').val();

                                                if ($.trim(key.toLowerCase()) === '') {
                                                    alert('Please enter a value or press Cancel.');
                                                    return false;
                                                }
                                                else if (phrase.length < 100) {
                                                    alert('Please enter a value at least 100 characters in length.');
                                                    return false;
                                                }

                                                $.ajax({
                                                    url: "/Default/PostCheckPhrase/",
                                                    type: "POST",
                                                    data: { 'value': phrase },
                                                    dataType: "json",
                                                    success: function (data) {

                                                    }
                                                });
                                            }
                                        },
                                        {
                                            // Cancel entry of phrase
                                            label: 'Cancel',
                                            action: function (dialogRef) {
                                                dialogRef.close();
                                                // TODO: redirect to home page
                                            }
                                        }
                                    ]
                                });
                            }
                            else
                            {
                                alert("Invalid response from server - please contact ISD support.");
                                // TODO: redirect to home page
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
                    // TODO: redirect to home page
                }
            }
        ]
    });
}