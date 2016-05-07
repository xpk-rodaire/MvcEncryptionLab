
function promptForKey() {
    BootstrapDialog.show({
        closable: false,
        size: BootstrapDialog.SIZE_LARGE,
        title: 'ACA-IRS File Processer',
        message: 'Please enter key: <input type="text" class="form-control">',
        buttons: [
            {
                label: 'OK',
                action: function (dialogRef) {
                    var key = dialogRef.getModalBody().find('input').val();
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
                                    message: 'Is this phrase correct? <input type="text" class="form-control" readonly value="' + data.checkPhrase + '">',
                                    buttons: [
                                        {
                                            label: 'Yes',
                                            action: function (dialogRef) {
                                                $.ajax({
                                                    url: "/Default/PostCheckPhraseResponse/",
                                                    type: "POST",
                                                    data: { 'value': true },
                                                    dataType: "json",
                                                    success: function (data) { }
                                                });
                                            }
                                        },
                                        {
                                            label: 'No',
                                            action: function (dialogRef) {
                                                $.ajax({
                                                    url: "/Default/PostCheckPhraseResponse/",
                                                    type: "POST",
                                                    data: { 'value': false },
                                                    dataType: "json",
                                                    success: function (data) { }
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
                                    message: 'Please enter pass phrase <input type="text" class="form-control"/>',
                                    buttons: [
                                        {
                                            label: 'OK',
                                            action: function (dialogRef) {
                                                var phrase = dialogRef.getModalBody().find('input').val();

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
                                            label: 'Cancel',
                                            // TODO: redirect to home page
                                            action: function (dialogRef) {

                                            }
                                        }
                                    ]
                                });
                            }
                            else
                            {
                                // TODO: redirect to home page
                                alert("Unknown status");
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
                label: 'Cancel',
                action: function (dialogRef) {
                    // TODO: redirect to home page
                    dialogRef.close();
                }
            }
        ]
    });
}