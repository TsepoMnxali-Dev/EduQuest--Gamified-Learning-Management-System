/*
   User page table data that will be replace with data from the database in the future */

const users = [

    {
        id: 1,
        firstName: "Emihle",
        surname: "Mtshawulana",
        email: "emihle.mtshawulana@EduQuest.com",
        role: "Learner",
        grade: 11,
        school: "KwaMagxaki High School",
        region: "Eastern Cape",
        status: "Active",

        registered: "15 March 2026",
        quizzesCompleted: 38,
        averageScore: 78,
        achievements: 7,
        competitions: 2
    },

    {
        id: 2,
        firstName: "Liqhamile",
        surname: "Silinga",
        email: "liqhamile.silinga@EduQuest.com",
        role: "Learner",
        grade: 12,
        school: "VP Boys High School",
        region: "Eastern Cape",
        status: "Active",

        registered: "20 March 2026",
        quizzesCompleted: 42,
        averageScore: 82,
        achievements: 9,
        competitions: 3
    },

    {
        id: 3,
        firstName: "Athenkosi",
        surname: "Bika",
        email: "athenkosi.bika@EduQuest.com",
        role: "Admin",
        grade: null,
        school: null,
        region: null,
        status: "Active",

        registered: "10 February 2026",
        quizzesUploaded: 24,
        resourcesUploaded: 18,
        competitionsCreated: 5
    }

];



/* full names */

function getFullName(user) {

    return `${user.firstName} ${user.surname}`;

}


/*GET INITIALS */
function getInitials(user) {

    return `${user.firstName.charAt(0)}${user.surname.charAt(0)}`.toUpperCase();

}

/* Hover card for the info of the learner*/

function createLearnerDetails(user) {

    return `
        <div class="user-details-card">

            <div class="user-details-header">

                <div class="large-avatar">
                    ${getInitials(user)}
                </div>

                <div>
                    <h3>${getFullName(user)}</h3>
                    <p>${user.email}</p>
                </div>

            </div>


            <div class="user-details-section">

                <h4>Academic Information</h4>

                <div class="user-detail-item">
                    <span>Grade</span>
                    <strong>Grade ${user.grade}</strong>
                </div>

                <div class="user-detail-item">
                    <span>School</span>
                    <strong>${user.school}</strong>
                </div>

                <div class="user-detail-item">
                    <span>Region</span>
                    <strong>${user.region}</strong>
                </div>

            </div>


            <div class="user-details-section">

                <h4>Activity</h4>

                <div class="user-detail-item">
                    <span>Registered</span>
                    <strong>${user.registered}</strong>
                </div>

                <div class="user-detail-item">
                    <span>Quizzes completed</span>
                    <strong>${user.quizzesCompleted}</strong>
                </div>

                <div class="user-detail-item">
                    <span>Average score</span>
                    <strong>${user.averageScore}%</strong>
                </div>

            </div>


            <div class="user-details-section">

                <h4>Achievements</h4>

                <div class="user-detail-item">
                    <span>Achievements earned</span>
                    <strong>${user.achievements}</strong>
                </div>

                <div class="user-detail-item">
                    <span>Competitions entered</span>
                    <strong>${user.competitions}</strong>
                </div>

            </div>

        </div>
    `;

}


/* hover on the table for the admin */

function createAdminDetails(user) {

    return `
        <div class="user-details-card">

            <div class="user-details-header">

                <div class="large-avatar">
                    ${getInitials(user)}
                </div>

                <div>
                    <h3>${getFullName(user)}</h3>
                    <p>${user.email}</p>
                </div>

            </div>


            <div class="user-details-section">

                <h4>Activity</h4>

                <div class="user-detail-item">
                    <span>Registered</span>
                    <strong>${user.registered}</strong>
                </div>

            </div>


            <div class="user-details-section">

                <h4>Content Management</h4>

                <div class="user-detail-item">
                    <span>Quizzes uploaded</span>
                    <strong>${user.quizzesUploaded}</strong>
                </div>

                <div class="user-detail-item">
                    <span>Resources uploaded</span>
                    <strong>${user.resourcesUploaded}</strong>
                </div>

                <div class="user-detail-item">
                    <span>Competitions created</span>
                    <strong>${user.competitionsCreated}</strong>
                </div>

            </div>

        </div>
    `;

}


/* main hover this function is to actually say that you must create the hover */

function createUserDetails(user) {

    if (user.role === "Learner") {

        return createLearnerDetails(user);

    }

    return createAdminDetails(user);

}


/* Display users in the table */

function displayUsers(userList = users) {

    const tableBody = document.getElementById("usersTableBody");

    if (!tableBody) {
        return;
    }

    tableBody.innerHTML = "";


    userList.forEach(user => {

        const row = document.createElement("tr");

        row.innerHTML = `

            <td class="user-name-column">

                <div class="user-name-cell user-hover-trigger">

                    <div class="avatar">
                        ${getInitials(user)}
                    </div>

                    <div class="user-name-information">

                        <strong>
                            ${getFullName(user)}
                        </strong>

                        <div class="user-email">
                            ${user.email}
                        </div>

                    </div>

                    ${createUserDetails(user)}

                </div>

            </td>


            <td>
                ${user.email}
            </td>


            <td>
                ${user.role}
            </td>


            <td>
                ${user.grade ? `Grade ${user.grade}` : "—"}
            </td>


            <td>
                ${user.school || "—"}
            </td>


            <td>
                ${user.region || "—"}
            </td>


            <td>
                ${user.status}
            </td>


            <td>

    <div class="row-actions">

        <button
            type="button"
            class="icon-action edit-action"
            onclick="editUser(${user.id})"
            title="Edit user"
            aria-label="Edit ${getFullName(user)}">

            <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                stroke-linecap="round"
                stroke-linejoin="round">

                <path d="M12 20h9"></path>

                <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L8 18l-4 1 1-4Z"></path>

            </svg>

        </button>


        <button
            type="button"
            class="icon-action delete-action"
            onclick="deleteUser(${user.id})"
            title="Delete user"
            aria-label="Delete ${getFullName(user)}">

            <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                stroke-linecap="round"
                stroke-linejoin="round">

                <polyline points="3 6 5 6 21 6"></polyline>

                <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path>

                <path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>

            </svg>

        </button>

    </div>

</td>

        `;

        tableBody.appendChild(row);

    });

}

/*SEARCH + FILTER USERS */

function applyUserFilters() {

    const searchInput = document.getElementById("userSearch");
    const roleFilter = document.getElementById("roleFilter");
    const gradeFilter = document.getElementById("gradeFilter");
    const statusFilter = document.getElementById("statusFilter");

    const searchValue = searchInput
        ? searchInput.value.toLowerCase().trim()
        : "";

    const selectedRole = roleFilter
        ? roleFilter.value
        : "all";

    const selectedGrade = gradeFilter
        ? gradeFilter.value
        : "all";

    const selectedStatus = statusFilter
        ? statusFilter.value
        : "all";


    const filteredUsers = users.filter(user => {

        /* Search */

        const fullName = getFullName(user).toLowerCase();

        const email = user.email.toLowerCase();

        const matchesSearch =
            fullName.includes(searchValue) ||
            email.includes(searchValue);


        /* Role */

        const matchesRole =
            selectedRole === "all" ||
            user.role.toLowerCase() === selectedRole.toLowerCase();


        /* Grade */

        const matchesGrade =
            selectedGrade === "all" ||
            String(user.grade) === selectedGrade;


        /* Status */

        const matchesStatus =
            selectedStatus === "all" ||
            user.status.toLowerCase() === selectedStatus.toLowerCase();


        return (
            matchesSearch &&
            matchesRole &&
            matchesGrade &&
            matchesStatus
        );

    });


    displayUsers(filteredUsers);
}


/* Search */

function searchUsers() {
    applyUserFilters();
}


/* Filters */

function filterUsers() {
    applyUserFilters();
}

/* delete the user or resource */

function deleteUser(userId) {

    const user = users.find(user => user.id === userId);


    if (!user) {
        return;
    }


    const confirmed = confirm(
    `Are you sure you want to delete ${getFullName(user)}?`
);


    if (!confirmed) {
        return;
    }


    const userIndex = users.findIndex(
        user => user.id === userId
    );


    if (userIndex !== -1) {

        users.splice(userIndex, 1);

        displayUsers();

    }

}

/* 
   EDIT USER
 */

function editUser(userId) {

    const user = users.find(
        user => user.id === userId
    );


    if (!user) {
        return;
    }


    /*
       For now this simply demonstrates
       that the correct user has been selected.

       Later this will open the Add/Edit User form
       and populate it with the user's information.
    */

    alert(
        `Edit user: ${getFullName(user)}`
    );

}

/* add */

function addUser(newUser) {

    const nextId = users.length > 0
        ? Math.max(...users.map(user => user.id)) + 1
        : 1;

    newUser.id = nextId;

    users.push(newUser);

    applyUserFilters();
}

/*
   ADD USER MODAL
 */

document.addEventListener("DOMContentLoaded", () => {

    const addUserButton = document.getElementById("openAddUser");
    const addUserModal = document.getElementById("addUserModal");
      console.log('button found:', addUserButton); 

    if (addUserButton && addUserModal) {

        addUserButton.addEventListener("click", () => {

            addUserModal.style.display = "flex";

        });

    }


    /* Submit Add User form */

    const addUserForm = addUserModal
        ? addUserModal.querySelector("form")
        : null;


    if (addUserForm) {

        addUserForm.addEventListener("submit", function(event) {

            event.preventDefault();


            const firstName =
                document.getElementById("firstName")?.value.trim();

            const surname =
                document.getElementById("surname")?.value.trim();

            const email =
                document.getElementById("email")?.value.trim();

            const role =
                document.getElementById("role")?.value;

            const grade =
                document.getElementById("grade")?.value;

            const school =
                document.getElementById("school")?.value.trim();

            const region =
                document.getElementById("region")?.value;

            const password =
                document.getElementById("password")?.value;

            const confirmPassword =
                document.getElementById("confirmPassword")?.value;

            const status =
                document.getElementById("status")?.value;


            /* Password validation */

            if (password !== confirmPassword) {

                alert("Passwords do not match.");

                return;

            }


            /* Create temporary user */

            const newUser = {

                firstName: firstName,

                surname: surname,

                email: email,

                role:
                    role === "admin"
                        ? "Admin"
                        : "Learner",

                grade:
                    role === "admin"
                        ? null
                        : Number(grade),

                school:
                    role === "admin"
                        ? null
                        : school,

                region:
                    role === "admin"
                        ? null
                        : region,

                status:
                    status
                        ? status.charAt(0).toUpperCase() + status.slice(1)
                        : "Active",

                registered: new Date().toLocaleDateString(
                    "en-GB",
                    {
                        day: "numeric",
                        month: "long",
                        year: "numeric"
                    }
                ),

                quizzesCompleted: 0,

                averageScore: 0,

                achievements: 0,

                competitions: 0

            };


            /* Add user to temporary JavaScript data */

            addUser(newUser);


            /* Close modal */

            addUserModal.style.display = "none";


            /* Reset form */

            addUserForm.reset();


            alert(
                `${getFullName(newUser)} has been added successfully.`
            );

        });

    }

});


document.addEventListener("DOMContentLoaded", () => {

    displayUsers();
    const addUserModal = document.getElementById("addUserModal");
    const closeAddUserButton = document.getElementById("closeAddUser");

    if (closeAddUserButton && addUserModal) {
        closeAddUserButton.addEventListener("click", () => {
            addUserModal.style.display = "none";
        });
    }

    const searchInput = document.getElementById("userSearch");
    const roleFilter = document.getElementById("roleFilter");
    const gradeFilter = document.getElementById("gradeFilter");
    const statusFilter = document.getElementById("statusFilter");

    if (searchInput) {
        searchInput.addEventListener("input", searchUsers);
    }

    if (roleFilter) {
        roleFilter.addEventListener("change", filterUsers);
    }

    if (gradeFilter) {
        gradeFilter.addEventListener("change", filterUsers);
    }

    if (statusFilter) {
        statusFilter.addEventListener("change", filterUsers);
    }
});
