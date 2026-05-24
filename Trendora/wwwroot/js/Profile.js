// Profile image preview and auto-upload
document.getElementById('profileImgInput').addEventListener('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        // Validate file size
        if (file.size > 5 * 1024 * 1024) {
            alert('File size must be less than 5MB!');
            this.value = '';
            return;
        }

        // Validate file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        if (!allowedTypes.includes(file.type)) {
            alert('Only image files (JPG, JPEG, PNG, GIF) are allowed!');
            this.value = '';
            return;
        }

        // Preview image
        const reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById('profileImgPreview').src = e.target.result;
        }
        reader.readAsDataURL(file);

        // Auto-submit the form
        document.getElementById('profileImageForm').submit();
    }
});

// Form validation
document.getElementById('accountForm').addEventListener('submit', function (e) {
    const fullName = document.querySelector('input[name="FullName"]').value;
    const email = document.querySelector('input[name="Email"]').value;

    if (!fullName.trim()) {
        e.preventDefault();
        alert('Full name is required');
        return;
    }

    if (!email.trim()) {
        e.preventDefault();
        alert('Email is required');
        return;
    }
});