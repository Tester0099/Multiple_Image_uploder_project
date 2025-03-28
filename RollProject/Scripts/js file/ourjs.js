
var Profile = document.getElementById("myprofile");
var info = document.getElementById("info");

var count = 0;
Profile.addEventListener("click", () => {
    if (count == 0) {
        info.style.display = "block ";
        count = 1;
    } else {
        info.style.display = "none ";
        count = 0;
    }
});

