$(document).ready(function () {

    function firstWidthDetect() {
        if (window.innerWidth >= 768) {
            $("#menu-toggle").html("<i id ='projectMenu' class='fas fa-less-than'></i>")
        } else if (window.innerWidth < 768) {
            $("#menu-toggle").html("<i id ='projectMenu' class='fas fa-greater-than'></i>")
            $("#wrapper").toggleClass("toggled");
        }
    }

    firstWidthDetect();

    function toggleWrapperOnWidthChange() {
        if (window.innerWidth >= 768 && $("#wrapper").hasClass("toggled")) {
            $("#wrapper").toggleClass("toggled");
        } else if (window.innerWidth < 768 && !$("#wrapper").hasClass("toggled")) {
            $("#wrapper").toggleClass("toggled");
        }
    }

    function toggleArrow() {
        if ($("#wrapper").hasClass("toggled")) {
            $("#menu-toggle").html("<i id ='projectMenu' class='fas fa-greater-than'></i>")
        } else {
            $("#menu-toggle").html("<i id='projectMenu' class='fas fa-less-than'></i>")
        }
    }

    function sizeChangeFunctions() {
        toggleWrapperOnWidthChange();
        toggleArrow();
    }

    window.onresize = sizeChangeFunctions;


    $("#menu-toggle").click(function (e) {
        e.preventDefault();
        $("#wrapper").toggleClass("toggled");
        if ($("#wrapper").hasClass("toggled")) {
            $("#menu-toggle").html("<i id='projectMenu' class='fas fa-greater-than'></i>")
        } else {
            $("#menu-toggle").html("<i id ='projectMenu' class='fas fa-less-than'></i>")
        }
    });

    var bugSeveritySelect = document.getElementById("bugSeverity")
    bugSeveritySelect.onchange = bugSeveritySelectBackground

    var bugStatusSelect = document.getElementById("bugStatus")
    bugStatusSelect.onchange = bugStatusSelectBackground

    function bugSeveritySelectBackground() {
        var bugSeveritySelectValue = bugSeveritySelect.value
        switch (bugSeveritySelectValue) {
            case "0":
                bugSeveritySelect.style.background = "#b81c04"
                break;
            case "1":
                bugSeveritySelect.style.background = "#d67404"
                break;
            default:
                bugSeveritySelect.style.background = "#ffee00"
        }
    }

    function bugStatusSelectBackground() {
        var bugStatusSelectValue = bugStatusSelect.value
        switch (bugStatusSelectValue) {
            case "0":
                bugStatusSelect.style.background = "#375a7f"
                break;
            case "1":
                bugStatusSelect.style.background = "#3498DB"
                break;
            default:
                bugStatusSelect.style.background = "#00bc8c"
        }
    }

    bugStatusSelectBackground()
    bugSeveritySelectBackground()

    var childElements = document.getElementById("links")
    var deleteLinks = document.getElementById("deleteScreenshot").getElementsByTagName("span");

    var screenshotSrc = [];
    for (let i = 0; i < childElements.childNodes.length; i++) {
        if (childElements.childNodes[i].nodeName == "IMG") {
            screenshotSrc.push(childElements.childNodes[i].src)
        }
    }

    while (childElements.firstChild) {
        childElements.removeChild(childElements.firstChild);
    }
    var deleteLinkDiv = document.getElementById("deleteScreenshot");
    deleteLinkDiv.parentNode.removeChild(deleteLinkDiv);


    if (deleteLinks.length == 0) {
        screenshotSrc.forEach((s, i) => {
            childElements.insertAdjacentHTML("beforeend",
                `<a class="screenshotdiv" href="${s}" >
                     <img class="screenshot" src="${s}" />
                </a>`
            )
        });

    } else {
        screenshotSrc.forEach((s, i) => {
            childElements.insertAdjacentHTML("beforeend",
                `<a class="screenshotdiv" href="${s}" >
                    <img class="screenshot" src="${s}" />
                    ${deleteLinks[i].outerHTML}
                </a>`
            )
        });
    }



    var deleteScreenShotSpan = document.getElementsByClassName("deleteScreenShot")
    for (let i = 0; i < deleteScreenShotSpan.length; i++) {
        deleteScreenShotSpan[i].addEventListener("click", function (e) {
            e.preventDefault();
            var action;
            var spanNode;
            if (e.target.tagName == "I") {
                action = e.target.parentNode.getAttribute("data-deleteAction")
                spanNode = e.target.parentNode
            } else {
                action = e.target.getAttribute("data-deleteAction")
                spanNode = e.target
            }

            $.ajax({
                type: "post",
                url: action,
            }).done(function (result) {
                if (result.status == "success") {
                    var screenShotToRemove = spanNode.parentNode;
                    screenShotToRemove.parentNode.removeChild(screenShotToRemove);
                }

            })

        })
    }



    document.getElementById('links').onclick = function (event) {
        event = event || window.event
        if (event.target.tagName == "IMG") {
            var target = event.target || event.srcElement,
                link = target.src ? target.parentNode : target,
                options = { index: link, event: event },
                links = this.getElementsByTagName('a')

            blueimp.Gallery(links, options)
        }
    }


    var deleteButton = document.getElementById("deleteButton")
    if (deleteButton != null) {
        deleteButton.onclick = function () {
            document.getElementById("deleteAlert").style.display = "block";
        }
    }

    var closeAlert = document.getElementById("deleteAlertClose")
    if (closeAlert != null) {
        closeAlert.onclick = function () {
            document.getElementById("deleteAlert").style.display = "none";

        }
    }

    var deletePhraseInput = document.getElementById("deletePhrase")
    if (deletePhraseInput != null) {
        deletePhraseInput.oninput = function (e) {
            var bugName = document.getElementById("bugName").value
            if (e.target.value == bugName) {
                document.getElementById("delete").style.display = "block";
            } else {
                document.getElementById("delete").style.display = "none";

            }
        }
    }

});