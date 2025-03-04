document.getElementById('register-button').addEventListener('click', async function(event) {
    event.preventDefault(); // Предотвращаем перезагрузку страницы

    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    const response = await fetch('http://localhost:5107/api/auth/register', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
    });

    if (response.ok) {
        const result = await response.json();
        alert(result.message); // "User registered successfully"
        window.location.href = 'index.html'; // Перенаправляем на главную
    } else {
        alert('Ошибка регистрации. Проверьте введённые данные.');
    }
});

document.getElementById('my-button').addEventListener('click', function() {
    window.location.href = 'my_page.html';
});