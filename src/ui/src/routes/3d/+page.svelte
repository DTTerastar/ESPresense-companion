<script lang="ts">
    import { onMount } from 'svelte';
    import * as THREE from 'three';
    import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
    import { CSS2DRenderer } from 'three/examples/jsm/renderers/CSS2DRenderer.js';
    import { GUI } from 'three/examples/jsm/libs/lil-gui.module.min.js';
    import Rooms3D from '$lib/3d/Rooms.svelte';
    import Nodes3D from '$lib/3d/Nodes.svelte';
    import Devices3D from '$lib/3d/Devices.svelte';
    import type { SvelteComponent } from 'svelte';

    let container: HTMLDivElement;
    let scene: THREE.Scene;
    let camera: THREE.PerspectiveCamera;
    let renderer: THREE.WebGLRenderer;
    let labelRenderer: CSS2DRenderer;
    let controls: OrbitControls;
    let groupPivot: THREE.Group;
    let isAnimating = false;
    let startTime: number;
    let devicesComponent: SvelteComponent & { update?: () => void };

    let showNodes = true;
    let zRotationSpeed = 0.002;

    const effectController = {
        zRotationSpeed: 0.002,
        showNodes: true,
        refreshNodes: () => {
            showNodes = false;
            setTimeout(() => {
                showNodes = true;
            }, 100);
        }
    };

    // Camera settings
    const CAM_START_X = 0;
    const CAM_START_Y = 0;
    const CAM_START_Z = 23;

    const CONTROLS_MIN_DISTANCE = 15;
    const CONTROLS_MAX_DISTANCE = 40;

    function initScene() {
        // Create renderer with basic WebGL settings
        renderer = new THREE.WebGLRenderer({
            antialias: true,
            powerPreference: "high-performance",
            stencil: false,
            depth: true
        });
        renderer.setPixelRatio(window.devicePixelRatio);
        renderer.setSize(container.clientWidth, container.clientHeight);
        renderer.setClearColor(0x1e293b, 1);
        renderer.autoClear = true;
        renderer.autoClearColor = true;
        renderer.autoClearDepth = true;
        container.appendChild(renderer.domElement);

        // Create and configure label renderer with proper stacking
        labelRenderer = new CSS2DRenderer();
        labelRenderer.setSize(container.clientWidth, container.clientHeight);
        labelRenderer.domElement.style.position = 'absolute';
        labelRenderer.domElement.style.top = '0px';
        labelRenderer.domElement.style.pointerEvents = 'none';
        labelRenderer.domElement.style.zIndex = '1';

        // Remove any existing label renderer elements
        const existingLabels = container.querySelector('.css2d-renderer');
        if (existingLabels) {
            container.removeChild(existingLabels);
        }

        labelRenderer.domElement.classList.add('css2d-renderer');
        container.appendChild(labelRenderer.domElement);

        scene = new THREE.Scene();
        scene.background = new THREE.Color(0x1e293b);

        camera = new THREE.PerspectiveCamera(45, container.clientWidth / container.clientHeight, 0.1, 1000);
        scene.add(camera);

        controls = new OrbitControls(camera, labelRenderer.domElement);
        controls.enableDamping = true;
        controls.dampingFactor = 0.05;
        controls.minDistance = CONTROLS_MIN_DISTANCE;
        controls.maxDistance = CONTROLS_MAX_DISTANCE;

        // Set up WebGL state
        const gl = renderer.getContext();
        gl.enable(gl.DEPTH_TEST);
        gl.depthFunc(gl.LEQUAL);

        groupPivot = new THREE.Group();
        scene.add(groupPivot);

        groupPivot.rotation.x = 5.2;
        groupPivot.rotation.z = 10.2;

        camera.position.set(CAM_START_X, CAM_START_Y, CAM_START_Z);
        controls.update();

        doGuiSetup();
    }

    function doGuiSetup() {
        const gui = new GUI({ title: 'Settings' });

        gui.add(effectController, 'zRotationSpeed', 0, 1, 0.01)
            .onChange((value: number) => { zRotationSpeed = value; });

//        gui.add(effectController, 'showNodes')
//            .onChange((value: boolean) => {
//              showNodes = value;
//            });

        gui.add(effectController, 'refreshNodes');
        gui.close();
    }

    function animate(currentTime: number) {
        if (!isAnimating) return;

        if (!startTime) startTime = currentTime;
        const elapsedTime = currentTime - startTime;

        controls.update();

        // Update rotation based on elapsed time
        groupPivot.rotation.z = (elapsedTime * zRotationSpeed * 0.001) % (Math.PI * 2);

        // Update device animations
        if (devicesComponent?.update) {
            devicesComponent.update();
        }

        // Ensure clean render state
        renderer.clear(true, true, true);  // Clear color, depth, and stencil

        // Render scene
        renderer.render(scene, camera);

        // Render labels on top
        labelRenderer.render(scene, camera);

        requestAnimationFrame(animate);
    }

    onMount(() => {
        if (container) {
            initScene();
            isAnimating = true;
            requestAnimationFrame(animate);
        }

        const handleResize = () => {
            if (!camera || !renderer || !labelRenderer) return;

            const width = container.clientWidth;
            const height = container.clientHeight;

            camera.aspect = width / height;
            camera.updateProjectionMatrix();

            renderer.setSize(width, height);
            renderer.setPixelRatio(window.devicePixelRatio);

            labelRenderer.setSize(width, height);
        };

        window.addEventListener('resize', handleResize);

        return () => {
            isAnimating = false;
            window.removeEventListener('resize', handleResize);
            if (renderer) {
                renderer.dispose();
                renderer.forceContextLoss();
            }
        };
    });
</script>

{#if groupPivot}
    <Rooms3D {groupPivot} />
    <Nodes3D {groupPivot} enabled={showNodes} />
    <Devices3D
        {groupPivot}
        bind:this={devicesComponent}
    />
{/if}

<div class="w-full h-full" bind:this={container}></div>

<style>
    div {
        background-color: rgb(30, 41, 59);
    }
</style>