<script lang="ts">
    import { onMount, onDestroy } from 'svelte';
    import { devices } from '$lib/stores';
    import * as THREE from 'three';
    import { CSS2DObject } from 'three/examples/jsm/renderers/CSS2DRenderer.js';
    import type { Group } from 'three';
    import type { Device } from '$lib/types';

    export let groupPivot: Group;

    // Position adjustments
    const X_POS_ADJ = 1.5;
    const Y_POS_ADJ = 5;

    // Animation settings
    const PULSE_SPEED = 2;  // Oscillations per second
    const PULSE_MIN = 0.8;
    const PULSE_MAX = 1.2;

    const geoSphere = new THREE.SphereGeometry(0.2, 32, 16);
    const trackerMaterials = [
        new THREE.MeshStandardMaterial({
            emissive: 0xff0000,
            emissiveIntensity: 2,
            transparent: true,
            opacity: 0.8
        }),
        new THREE.MeshStandardMaterial({
            emissive: 0xffbb00,
            emissiveIntensity: 2,
            transparent: true,
            opacity: 0.8
        }),
        new THREE.MeshStandardMaterial({
            emissive: 0xffee00,
            emissiveIntensity: 2,
            transparent: true,
            opacity: 0.8
        }),
    ];

    let deviceGroup: THREE.Group | null = null;
    let trackingSpheres: THREE.Mesh[] = [];
    let trackerLabels: { [key: string]: HTMLDivElement } = {};
    let startTime = performance.now();

    $: if ($devices && groupPivot) {
        updateDevices($devices);
    }

    function cleanupDeviceGroup() {
        if (!deviceGroup) return;

        deviceGroup.traverse(child => {
            if ((child as any).geometry) {
                (child as any).geometry.dispose();
            }
            if ((child as any).material) {
                (child as any).material.dispose();
            }
            // Clean up CSS2D labels
            if (child instanceof CSS2DObject) {
                const element = child.element;
                if (element && element.parentNode) {
                    element.parentNode.removeChild(element);
                }
            }
        });

        groupPivot.remove(deviceGroup);
        deviceGroup = null;
        trackingSpheres = [];
        trackerLabels = {};
    }

    function updateDevices(devices: Device[]) {
        cleanupDeviceGroup();

        const newDeviceGroup = new THREE.Group();
        newDeviceGroup.name = 'DeviceGroup';

        devices.forEach(device => {
            if (!device.location) return;

            const trackName = device.id;
            const confidence = device.confidence || 0;
            const fixes = device.fixes || 0;
            const position = device.location;

            if (confidence <= 1) return;

            const material = trackerMaterials[trackingSpheres.length % trackerMaterials.length];
            const newSphere = new THREE.Mesh(geoSphere, material);
            newSphere.name = trackName;
            newSphere.position.set(position.x - X_POS_ADJ, position.y - Y_POS_ADJ, position.z);

            trackingSpheres.push(newSphere);
            newDeviceGroup.add(newSphere);

            const labelDivEle = document.createElement('div');
            labelDivEle.style.color = '#ffffff';
            labelDivEle.style.fontFamily = 'Arial';
            labelDivEle.style.fontSize = '0.8rem';
            labelDivEle.style.marginTop = '-1em';

            const labelDivLine1 = document.createElement('div');
            const displayName = device.name || device.id;
            labelDivLine1.textContent = displayName.length > 15 ? (displayName.substring(0, 14) + '...') : displayName;

            const labelDivLine2 = document.createElement('div');
            labelDivLine2.textContent = `${confidence}% (${fixes} fixes)`;

            labelDivEle.append(labelDivLine1, labelDivLine2);
            trackerLabels[trackName] = labelDivLine2;

            const labelElement = new CSS2DObject(labelDivEle);
            labelElement.name = trackName + '#label';
            labelElement.position.set(position.x - X_POS_ADJ, position.y - Y_POS_ADJ, position.z);

            newDeviceGroup.add(labelElement);
        });

        deviceGroup = newDeviceGroup;
        groupPivot.add(deviceGroup);
    }

    // Update pulse scale based on time
    function updatePulse() {
        const elapsed = (performance.now() - startTime) / 1000;
        const phase = (elapsed * PULSE_SPEED * Math.PI) % (Math.PI * 2);
        const scale = PULSE_MIN + (Math.sin(phase) + 1) * (PULSE_MAX - PULSE_MIN) / 2;

        trackingSpheres.forEach((sphere) => {
            sphere.scale.set(scale, scale, scale);
        });
    }

    // Export update function for parent to call during animation
    export function update() {
        if (deviceGroup) {
            updatePulse();
        }
    }

    onDestroy(() => {
        cleanupDeviceGroup();
    });
</script>