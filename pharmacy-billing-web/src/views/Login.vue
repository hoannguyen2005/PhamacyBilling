<template>
  <div class="login-card">
    <div class="login-logo">
      <span class="icon">🏥</span>
      <h2>Phòng Khám Đa Khoa</h2>
      <p>Hệ thống Quản lý Dược & Viện Phí</p>
    </div>

    <!-- Alert Panel -->
    <div v-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <label class="form-label" for="username">Tên đăng nhập</label>
        <input 
          v-model="username" 
          type="text" 
          id="username" 
          class="form-input" 
          placeholder="Nhập tên đăng nhập (ví dụ: admin)" 
          required 
          autocomplete="username"
        />
      </div>

      <div class="form-group">
        <label class="form-label" for="password">Mật khẩu</label>
        <input 
          v-model="password" 
          type="password" 
          id="password" 
          class="form-input" 
          placeholder="Nhập mật khẩu" 
          required 
          autocomplete="current-password"
        />
      </div>

      <button type="submit" class="btn btn-primary" :disabled="loading" style="width: 100%; justify-content: center; margin-top: 1rem;">
        {{ loading ? 'Đang xác thực...' : '🔑 Đăng Nhập' }}
      </button>
    </form>

    <div style="margin-top: 1.5rem; text-align: center; font-size: 0.8rem; color: #64748b;">
      <p>Tài khoản dùng thử:</p>
      <p><b>admin</b> / <b>admin123</b> (Quản trị)</p>
      <p><b>receptionist1</b> / <b>reception123</b> (Thu ngân)</p>
      <p><b>doctor1</b> / <b>doctor123</b> (Bác sĩ)</p>
    </div>
  </div>
</template>

<script>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';

export default {
  name: 'Login',
  setup() {
    const router = useRouter();
    const username = ref('');
    const password = ref('');
    const error = ref('');
    const loading = ref(false);

    const handleLogin = async () => {
      loading.value = true;
      error.value = '';

      try {
        const response = await api.post('/auth/login', {
          username: username.value,
          password: password.value
        });

        const { token, user } = response.data;
        
        // Save auth data locally
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(user));

        // Redirect based on role
        if (user.roleName === 'Admin') {
          router.push('/');
        } else {
          router.push('/medicines');
        }
      } catch (err) {
        if (err.response && err.response.data && err.response.data.message) {
          error.value = err.response.data.message;
        } else {
          error.value = 'Không thể kết nối đến máy chủ API. Vui lòng kiểm tra lại.';
        }
      } finally {
        loading.value = false;
      }
    };

    return {
      username,
      password,
      error,
      loading,
      handleLogin
    };
  }
};
</script>
