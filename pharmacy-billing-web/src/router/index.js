import { createRouter, createWebHashHistory } from 'vue-router';
import Login from '../views/Login.vue';
import Dashboard from '../views/Dashboard.vue';
import Medicines from '../views/Medicines.vue';
import Prescriptions from '../views/Prescriptions.vue';
import Bills from '../views/Bills.vue';

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { public: true }
  },
  {
    path: '/',
    name: 'Dashboard',
    component: Dashboard,
    meta: { roles: ['Admin'] }
  },
  {
    path: '/medicines',
    name: 'Medicines',
    component: Medicines,
    meta: { roles: ['Admin', 'Doctor', 'Receptionist'] }
  },
  {
    path: '/prescriptions',
    name: 'Prescriptions',
    component: Prescriptions,
    meta: { roles: ['Admin', 'Doctor', 'Receptionist'] }
  },
  {
    path: '/bills',
    name: 'Bills',
    component: Bills,
    meta: { roles: ['Admin', 'Receptionist'] }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
];

const router = createRouter({
  history: createWebHashHistory(),
  routes
});

// Navigation Guard
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token');
  const userJson = localStorage.getItem('user');
  let user = null;

  if (userJson) {
    try {
      user = JSON.parse(userJson);
    } catch (e) {
      localStorage.removeItem('user');
      localStorage.removeItem('token');
    }
  }

  // Check if route is public
  if (to.meta.public) {
    if (token && user) {
      // Already logged in, redirect based on role
      if (user.roleName === 'Admin') {
        next('/');
      } else {
        next('/medicines');
      }
    } else {
      next();
    }
    return;
  }

  // Not logged in
  if (!token || !user) {
    next('/login');
    return;
  }

  // Check role permission
  if (to.meta.roles && !to.meta.roles.includes(user.roleName)) {
    // Insufficient permissions, redirect based on what role is
    alert('Bạn không có quyền truy cập chức năng này!');
    if (user.roleName === 'Admin') {
      next('/');
    } else {
      next('/medicines');
    }
    return;
  }

  next();
});

export default router;
