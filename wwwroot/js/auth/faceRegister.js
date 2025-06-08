
function initRegisterFace(userId) {
   
    const video = document.getElementById('video');
    const canvas = document.getElementById('canvas');
    const captureButton = document.getElementById('capture');
    const cancelButton = document.getElementById('cancel');
    const loadingSpinner = document.querySelector('.loading-spinner');
    const btnText = document.querySelector('.btn-text');
    const processSteps = document.getElementById('processSteps');
    const steps = document.querySelectorAll('.step');

    // Configure toastr
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "timeOut": "3000"
    };

    // Function to update step status
    function updateStep(stepNumber) {
        steps.forEach((step, index) => {
            if (index + 1 === stepNumber) {
                step.classList.add('active');
            } else {
                step.classList.remove('active');
            }
        });
    }

    navigator.mediaDevices.getUserMedia({ video: true })
        .then(stream => {
            video.srcObject = stream;
        })
        .catch(err => {
            toastr.error('Không truy cập được webcam: ' + err);
        });

    captureButton.addEventListener('click', async () => {
        // Show loading state
        loadingSpinner.style.display = 'inline-block';
        btnText.textContent = 'Đang xử lý...';
        captureButton.disabled = true;
        processSteps.style.display = 'block';

        try {
            // Step 1: Capture image
            updateStep(1);
            toastr.info('📸 Đang chụp ảnh từ webcam...');
            canvas.getContext('2d').drawImage(video, 0, 0, 320, 240);
            await new Promise(resolve => setTimeout(resolve, 500)); // Small delay for visual feedback

            // Step 2: Convert to blob
            updateStep(2);
            toastr.info('🧪 Đang chuyển ảnh sang định dạng blob...');
            const imageData = canvas.toDataURL('image/jpeg');
            const blob = await (await fetch(imageData)).blob();
            await new Promise(resolve => setTimeout(resolve, 500)); // Small delay for visual feedback
            userId = document.getElementById("auth-card").dataset.userId;
            // Step 3: Prepare data
            updateStep(3);
            toastr.info('📦 Chuẩn bị gửi dữ liệu đến server...');
            const formData = new FormData();
            formData.append('image', blob, 'faceImage.jpg');
            formData.append('userId', userId);
            await new Promise(resolve => setTimeout(resolve, 500)); // Small delay for visual feedback

            // Step 4: Send to server
            updateStep(4);
            toastr.info('📬 Đã gửi xong, đang chờ phản hồi...');
            const response = await fetch('/Auth/RegisterFace', {
                method: 'POST',
                body: formData
            });
            const result = await response.json();

            if (result.success) {
                toastr.success('✅ ' + result.message);
                setTimeout(() => { window.location.href = '/Admin/Auth'; }, 1000);
            } else {
                toastr.warning('⚠️ ' + result.message);
            }
        } catch (error) {
            toastr.error('❌ Lỗi khi gửi yêu cầu: ' + error);
            console.error(error);
        } finally {
            // Reset button state
            loadingSpinner.style.display = 'none';
            btnText.textContent = 'Chụp ảnh';
            captureButton.disabled = false;
        }
    });
    


    cancelButton.addEventListener('click', function () {
        container1.style.display = 'none';

    });

}